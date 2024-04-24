using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AokExporter.Features.PatientReceipts.Models;
using Microsoft.Extensions.Logging;

namespace AokExporter.Features.PatientReceipts;

public class AokClient
{
    public const string HttpClientName = nameof(AokClient);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AokClient> _logger;
    private string _accessToken;
    private string _clientId;
    private string _refreshToken;

    public AokClient(IHttpClientFactory httpClientFactory, ILogger<AokClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient()
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        return httpClient;
    }

    public void InitTokens(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;

        var token = new JwtSecurityToken(accessToken);
        _clientId = token.Claims.First(x => x.Type == "client_id").Value;
    }

    public async Task RefreshTokenAsync()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("scope", "openid offline_access urn:telematik:geburtsdatum"),
            new KeyValuePair<string, string>("refresh_token", _refreshToken),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("audience", "pkogs")
        });

        var url = "/cx/pk-ogs-portal/occ/v2/pop/auth/token";

        var httpClient = CreateClient();
        var response = await httpClient.PostAsync(url, content);

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (tokenResponse is null)
            throw new InvalidDataException("Can't read invalid token response");

        _accessToken = tokenResponse.AccessToken;
        _refreshToken = tokenResponse.RefreshToken;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        _logger.LogDebug("Refreshed Token");
    }

    public async Task<IReadOnlyList<Quarter>> GetQuartersAsync()
    {
        var url = "/cx/epq/quarters";
        var httpClient = CreateClient();
        var response = await httpClient.GetAsync(url);
        var quartersResponse = await response.Content.ReadFromJsonAsync<QuartersResponse>();

        if (quartersResponse is null)
            throw new InvalidDataException("Can't read invalid quarters response");

        return quartersResponse.Quarters
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyList<ServiceGroup>> GetServiceGroupsAsync(string from, string to)
    {
        var url = $"/cx/epq/service-groups?dateFrom={from}&dateTo={to}";
        var httpClient = CreateClient();
        var response = await httpClient.GetAsync(url);
        var serviceGroupsResponse = await response.Content.ReadFromJsonAsync<ServiceGroupsResponse>();

        if (serviceGroupsResponse is null)
            throw new InvalidDataException("Can't read invalid servicegroups response");

        return serviceGroupsResponse.Results
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyList<CaseResponse>> GetCasesAsync(string serviceGroupId, string from, string to)
    {
        var url = $"/cx/epq/service-groups/{serviceGroupId}/cases?dateFrom={from}&dateTo={to}";
        var httpClient = CreateClient();
        var response = await httpClient.GetAsync(url);
        var cases = await response.Content.ReadFromJsonAsync<IList<CaseResponse>>();

        if (cases is null)
            throw new InvalidDataException("Can't read invalid cases response");

        return cases.AsReadOnly();
    }

    public async Task<CaseDetailsResponse> GetCaseDetailsAsync(string serviceGroupId, string caseId, string caseSource)
    {
        var url = $"/cx/epq/service-groups/{serviceGroupId}/cases/{caseId}?caseSource={caseSource}";
        var httpClient = CreateClient();
        var response = await httpClient.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.Unauthorized) //Sometimes there is no access to the case details :(
            return null;
        
        var caseResponse = await response.Content.ReadFromJsonAsync<CaseDetailsResponse>();

        if (caseResponse is null)
            throw new InvalidDataException("Can't read invalid case response");

        return caseResponse;
    }
}