using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record QuartersResponse
{
    [JsonPropertyName("defaultFrom")] public required string DefaultFrom { get; init; }
    [JsonPropertyName("defaultTo")] public required string DefaultTo { get; init; }
    [JsonPropertyName("quarters")] public required IEnumerable<Quarter> Quarters { get; init; }
}