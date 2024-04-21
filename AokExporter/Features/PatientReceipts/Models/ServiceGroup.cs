using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record ServiceGroup
{
    [JsonPropertyName("serviceGroupId")] public required string Id { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("currency")] public required string Currency { get; init; }
    [JsonPropertyName("ownPayment")] public required decimal OwnPayment { get; init; }
    [JsonPropertyName("total")] public required decimal Total { get; init; }
    [JsonPropertyName("percentage")] public required float Percentage { get; init; }
}