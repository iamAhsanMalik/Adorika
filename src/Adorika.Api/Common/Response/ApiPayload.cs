namespace Adorika.Api.Common.Response;

// 6. SHARED MODELS
public record ApiPayload<T>(
    string Message,
    T? Data,
    object? Errors,
    List<ResultLink> Links,
    DateTimeOffset Timestamp,
    string MachineName,
    string TraceId,
    string RequestId
);
