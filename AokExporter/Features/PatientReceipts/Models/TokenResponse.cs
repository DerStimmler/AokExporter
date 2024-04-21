using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record TokenResponse
{
    [JsonPropertyName("access_token")] public required string AccessToken { get; init; }

    [JsonPropertyName("id_token")] public required string IdToken { get; init; }

    [JsonPropertyName("refresh_token")] public required string RefreshToken { get; init; }

    [JsonPropertyName("uid")] public required string UId { get; init; }
}