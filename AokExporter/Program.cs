using System.Net;
using System.Text.Json;
using AokExporter.Features.PatientReceipts;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

var builder = CoconaApp.CreateBuilder();

builder.Services.AddSingleton<AokClient>();
builder.Services.AddHttpClient(AokClient.HttpClientName, httpClient => { httpClient.BaseAddress = new Uri("https://app-api.meine.aok.de"); })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler((serviceProvider, _) => GetRefreshTokenPolicy(serviceProvider));

var app = builder.Build();

app.AddCommand(async ([Option('a')] string accessToken, [Option('r')] string refreshToken, AokClient client) =>
{
    var jsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    client.InitTokens(accessToken, refreshToken);

    var quaters = await client.GetQuartersAsync();
    var minFrom = quaters.Min(quater => quater.ValueFrom);
    var maxTo = quaters.Max(quater => quater.ValueTo);

    var serviceGroups = await client.GetServiceGroupsAsync(minFrom, maxTo);

    foreach (var serviceGroup in serviceGroups)
    {
        var cases = await client.GetCasesAsync(serviceGroup.Id, minFrom, maxTo);

        foreach (var @case in cases)
        {
            var caseDetails = await client.GetCaseDetailsAsync(serviceGroup.Id, @case.Id, @case.Source);

            var path = $"./export/{serviceGroup.Name}/{@case.Name}";
            path = string.Join("_", path.Split(Path.GetInvalidPathChars()));
            path = path.Split("(").First();

            var directory = Directory.CreateDirectory(path);
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, $"{@case.Id}.json"), JsonSerializer.Serialize(@case, jsonSerializerOptions));
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, $"{@case.Id}-details.json"), caseDetails);
            
            Console.WriteLine($"Exported: {serviceGroup.Name}/{@case.Name}/{@case.Id}");
        }
    }
    
    Console.WriteLine("FINISH");
});

app.Run();
return;

static IAsyncPolicy<HttpResponseMessage> GetRefreshTokenPolicy(IServiceProvider serviceProvicer)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized && !msg.RequestMessage!.RequestUri!.ToString().EndsWith("token"))
        .WaitAndRetryAsync(1,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (_, _) =>
            {
                var client = serviceProvicer.GetRequiredService<AokClient>();
                return client.RefreshTokenAsync();
            });
}