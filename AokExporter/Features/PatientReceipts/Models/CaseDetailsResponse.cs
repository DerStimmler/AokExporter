using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record CaseDetailsResponse
{
    [JsonPropertyName("tiles")] public required IEnumerable<CaseDetailsFieldResponse> Tiles { get; init; }
    [JsonPropertyName("rows")] public required IEnumerable<IEnumerable<CaseDetailsFieldResponse>> Rows { get; init; }
    [JsonPropertyName("summary")] public required IEnumerable<CaseDetailsFieldResponse> Summary { get; init; }
}