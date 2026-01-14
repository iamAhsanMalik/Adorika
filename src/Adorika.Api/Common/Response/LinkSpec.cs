namespace Adorika.Api.Common.Response;

// 2. LINK SPEC
public record LinkSpec(
    string RouteName,
    string Rel,
    string Method = "GET",
    object? Values = null,
    string? Title = null,
    string? Type = null);
