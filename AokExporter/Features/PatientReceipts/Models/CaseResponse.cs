using System.Text.Json.Serialization;

namespace AokExporter.Features.PatientReceipts.Models;

public record CaseResponse
{
    [JsonPropertyName("caseSource")] public required string Source { get; init; }
    [JsonPropertyName("caseId")] public required string Id { get; init; }
    [JsonPropertyName("providerId")] public required string ProviderId { get; init; }
    [JsonPropertyName("dentistId")] public required string DentistId { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("dateFrom")] public required string DateFrom { get; init; }
    [JsonPropertyName("dateTo")] public required string DateTo { get; init; }
    [JsonPropertyName("cost")] public required decimal Cost { get; init; }
    [JsonPropertyName("ownPayment")] public required decimal OwnPayment { get; init; }
    [JsonPropertyName("currency")] public required string Currency { get; init; }
    [JsonPropertyName("typeCode")] public required string TypeCode { get; init; }
}