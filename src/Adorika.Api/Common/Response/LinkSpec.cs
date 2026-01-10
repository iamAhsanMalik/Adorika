namespace Adorika.Api.Common.Wrapper;

// 2. LINK SPEC
public record LinkSpec(
    string RouteName,
    string Rel,
    string Method = "GET",
    object? Values = null,
    string? Title = null,
    string? Type = null);
