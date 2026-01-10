namespace Adorika.Application.Common.Wrapper;

public record ResultError(
    string Code,
    string Message,
    int StatusCode = 400)
{
    // 4xx Client Errors

    public static ResultError ValidationError(string code, string message) =>
        new($"{code}.VALIDATION_ERROR", message, 400);

    public static ResultError BadRequest(string code, string message) =>
        new($"{code}.BAD_REQUEST", message, 400);

    public static ResultError Unauthorized(string code, string message) =>
        new($"{code}.UNAUTHORIZED", message, 401);

    public static ResultError Forbidden(string code, string message) =>
        new($"{code}.FORBIDDEN", message, 403);

    public static ResultError NotFound(string code, string message) =>
        new($"{code}.RESOURCE_NOT_FOUND", $"{message} not found", 404);

    public static ResultError MethodNotAllowed(string code, string message) =>
        new($"{code}.METHOD_NOT_ALLOWED", message, 405);

    public static ResultError NotAcceptable(string code, string message) =>
        new($"{code}.NOT_ACCEPTABLE", message, 406);

    public static ResultError RequestTimeout(string code, string message) =>
        new($"{code}.REQUEST_TIMEOUT", message, 408);

    public static ResultError Conflict(string code, string message) =>
        new($"{code}.RESOURCE_CONFLICT", message, 409);

    public static ResultError Gone(string code, string message) =>
        new($"{code}.RESOURCE_GONE", message, 410);

    public static ResultError UnsupportedMediaType(string code, string message) =>
        new($"{code}.UNSUPPORTED_MEDIA_TYPE", message, 415);

    public static ResultError BusinessRuleViolation(string code, string message) =>
        new($"{code}.BUSINESS_RULE_VIOLATION", message, 422);

    public static ResultError InvalidOperation(string code, string message) =>
        new($"{code}.INVALID_OPERATION", message, 422);

    public static ResultError DataIntegrityViolation(string code, string message) =>
        new($"{code}.DATA_INTEGRITY_VIOLATION", message, 422);

    public static ResultError ResourceLocked(string code, string message) =>
        new($"{code}.RESOURCE_LOCKED", message, 423);

    public static ResultError DependencyFailed(string code, string message) =>
        new($"{code}.DEPENDENCY_FAILED", message, 424);

    public static ResultError RateLimitExceeded(string code, string message) =>
        new($"{code}.RATE_LIMIT_EXCEEDED", message, 429);

    public static ResultError TooManyRequests(string code, string message) =>
        new($"{code}.TOO_MANY_REQUESTS", message, 429);

    public static ResultError QuotaExceeded(string code, string message) =>
        new($"{code}.QUOTA_EXCEEDED", message, 429);

    // Authentication / Authorization

    public static ResultError SessionExpired(string code, string message) =>
        new($"{code}.SESSION_EXPIRED", message, 401);

    public static ResultError InvalidToken(string code, string message) =>
        new($"{code}.INVALID_TOKEN", message, 401);

    public static ResultError TokenExpired(string code, string message) =>
        new($"{code}.TOKEN_EXPIRED", message, 401);

    // Payment / Account

    public static ResultError InsufficientFunds(string code, string message) =>
        new($"{code}.INSUFFICIENT_FUNDS", message, 402);

    public static ResultError AccountSuspended(string code, string message) =>
        new($"{code}.ACCOUNT_SUSPENDED", message, 403);

    // 5xx Server Errors

    public static ResultError InternalError(string code, string message) =>
        new($"{code}.INTERNAL_ERROR", message, 500);

    public static ResultError ConfigurationError(string code, string message) =>
        new($"{code}.CONFIGURATION_ERROR", message, 500);

    public static ResultError NotImplemented(string code, string message) =>
        new($"{code}.NOT_IMPLEMENTED", message, 501);

    public static ResultError FeatureDisabled(string code, string message) =>
        new($"{code}.FEATURE_DISABLED", message, 501);

    public static ResultError BadGateway(string code, string message) =>
        new($"{code}.BAD_GATEWAY", message, 502);

    public static ResultError ExternalServiceError(string code, string message) =>
        new($"{code}.EXTERNAL_SERVICE_ERROR", message, 502);

    public static ResultError ServiceUnavailable(string code, string message) =>
        new($"{code}.SERVICE_UNAVAILABLE", message, 503);

    public static ResultError MaintenanceMode(string code, string message) =>
        new($"{code}.MAINTENANCE_MODE", message, 503);

    public static ResultError GatewayTimeout(string code, string message) =>
        new($"{code}.GATEWAY_TIMEOUT", message, 504);

    public static ResultError HttpVersionNotSupported(string code, string message) =>
        new($"{code}.HTTP_VERSION_NOT_SUPPORTED", message, 505);

    public static ResultError InsufficientStorage(string code, string message) =>
        new($"{code}.INSUFFICIENT_STORAGE", message, 507);

    public static ResultError NetworkAuthenticationRequired(string code, string message) =>
        new($"{code}.NETWORK_AUTHENTICATION_REQUIRED", message, 511);
}
