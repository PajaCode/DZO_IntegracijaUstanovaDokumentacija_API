using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Api;

public sealed record OperationSummary
{
    public ApiExecutionStatus Status { get; set; } = ApiExecutionStatus.Succeeded;
    public string? Message { get; set; }

    public int Total { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }

    public List<ItemError> Errors { get; } = new();

    [System.Text.Json.Serialization.JsonIgnore]
    public bool Success => Status == ApiExecutionStatus.Succeeded;
}
