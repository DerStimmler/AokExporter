using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record CaseDetailsFieldResponse
{
    [JsonPropertyName("fieldLabel")] public required string FieldLabel { get; init; }
    [JsonPropertyName("fieldId")] public required string FieldId { get; init; }
    [JsonPropertyName("fields")] public required IEnumerable<string> Fields { get; init; }

}