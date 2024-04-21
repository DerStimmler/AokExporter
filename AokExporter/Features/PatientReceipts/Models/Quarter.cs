using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record Quarter
{
    [JsonPropertyName("valueFrom")] public required string ValueFrom { get; init; }
    [JsonPropertyName("valueTo")] public required string ValueTo { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
}