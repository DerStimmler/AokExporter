using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record ServiceGroupsResponse
{
    [JsonPropertyName("currency")] public required string Currency { get; init; }
    [JsonPropertyName("ownPayment")] public required decimal OwnPayment { get; init; }
    [JsonPropertyName("total")] public required decimal Total { get; init; }
    [JsonPropertyName("results")] public required IEnumerable<ServiceGroup> Results { get; init; }
}