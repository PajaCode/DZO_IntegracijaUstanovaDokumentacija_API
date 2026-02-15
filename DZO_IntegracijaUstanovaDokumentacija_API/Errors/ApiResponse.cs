using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using Microsoft.AspNetCore.Http;
using System;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Api;

public sealed record ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string CorrelationId { get; set; } = "";
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

    public T? Data { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(HttpContext http, T data, string? message = null)
        => new()
        {
            Success = true,
            Message = message,
            CorrelationId = http.TraceIdentifier,
            Data = data
        };

    public static ApiResponse<T> Fail(HttpContext http, string code, string message, T? data = default)
        => new()
        {
            Success = false,
            Message = message,
            CorrelationId = http.TraceIdentifier,
            Error = new ApiError(code, message),
            Data = data
        };

    public static ApiResponse<OperationSummary> FromOperation(HttpContext http, OperationSummary summary)
    {
        var ok = summary.Status == ApiExecutionStatus.Succeeded;

        return new ApiResponse<OperationSummary>
        {
            Success = ok,
            Message = summary.Message,
            CorrelationId = http.TraceIdentifier,
            Data = summary,
            Error = ok ? null : new ApiError("COMPLETED_WITH_ERRORS", summary.Message ?? "Operacija nije uspela u potpunosti")
        };
    }
}
