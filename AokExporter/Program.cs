using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using AokExporter.Features.PatientReceipts;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

const string dateRegex = @"^\d{4}-\d{2}-\d{2}$";

var builder = CoconaApp.CreateBuilder();

builder.Logging
    .SetMinimumLevel(LogLevel.Information)
    .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services.AddSingleton<AokClient>();
builder.Services.AddHttpClient(AokClient.HttpClientName, httpClient => httpClient.BaseAddress = new Uri("https://app-api.meine.aok.de"))
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler((serviceProvider, _) => GetRefreshTokenPolicy(serviceProvider));

var app = builder.Build();

app.AddCommand(async (
    AokClient client,
    ILogger<Program> logger,
    [Option('a')] string accessToken,
    [Option('r')] string refreshToken,
    [Option('f')] [RegularExpression(dateRegex)]
    string? from,
    [Option('t')] [RegularExpression(dateRegex)]
    string? to,
    [Option('o')] string outputDirectory = "./export"
) =>
{
    logger.LogInformation("Output directory: {OutputDirectory}", outputDirectory);

    var jsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    client.InitTokens(accessToken, refreshToken);

    if (from is null || to is null)
    {
        var quaters = await client.GetQuartersAsync();

        from ??= quaters.Min(quater => quater.ValueFrom);
        to ??= quaters.Max(quater => quater.ValueTo);
    }

    logger.LogInformation("Get data from '{From}' to '{To}'", from, to);

    var serviceGroups = await client.GetServiceGroupsAsync(from, to);

    logger.LogInformation("Found service groups: {ServiceGroups}", string.Join(", ", serviceGroups.Select(x => x.Name)));

    foreach (var serviceGroup in serviceGroups)
    {
        var escapedServiceGroupName = EscapePath(serviceGroup.Name).Split("(").First();

        var serviceGroupPath = Path.Combine(outputDirectory, escapedServiceGroupName);
        Directory.CreateDirectory(serviceGroupPath);

        await File.WriteAllTextAsync(Path.Combine(serviceGroupPath, $"{escapedServiceGroupName}.json"), JsonSerializer.Serialize(serviceGroup, jsonSerializerOptions));

        var cases = await client.GetCasesAsync(serviceGroup.Id, from, to);

        foreach (var @case in cases)
        {
            var caseDetails = await client.GetCaseDetailsAsync(serviceGroup.Id, @case.Id, @case.Source);

            var escapedCaseName = EscapePath(@case.Name).Split("(").First();

            var casePath = Path.Combine(serviceGroupPath, escapedCaseName);

            Directory.CreateDirectory(casePath);

            var enrichedCase = ToDynamic(@case);
            enrichedCase.Details = caseDetails;

            await File.WriteAllTextAsync(Path.Combine(casePath, $"{@case.DateFrom}_{@case.Id}.json"), JsonSerializer.Serialize(enrichedCase, jsonSerializerOptions));

            logger.LogInformation("Exported: {ServiceGroupName}/{CaseName}/{CaseFrom}_{CaseId}", serviceGroup.Name, @case.Name, @case.DateFrom, @case.Id);
        }
    }

    logger.LogInformation("FINISH");
});

app.Run();
return;

static string EscapePath(string path)
{
    var charsToReplace = Path.GetInvalidPathChars()
        .Concat([' '])
        .ToArray();
    
    return string.Join("_", path.Split(charsToReplace)).Trim();
}

static IAsyncPolicy<HttpResponseMessage> GetRefreshTokenPolicy(IServiceProvider serviceProvider)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized && !msg.RequestMessage!.RequestUri!.ToString().EndsWith("token"))
        .WaitAndRetryAsync(1,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (_, _) =>
            {
                var client = serviceProvider.GetRequiredService<AokClient>();
                return client.RefreshTokenAsync();
            });
}

static dynamic ToDynamic<T>(T obj)
{
    var expando = new ExpandoObject();

    foreach (var propertyInfo in typeof(T).GetProperties())
        expando.TryAdd(propertyInfo.Name, propertyInfo.GetValue(obj));

    return expando;
}