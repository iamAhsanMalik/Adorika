namespace Adorika.Api.Common.Wrapper;

// 1. CLEANED RESULT LINK
public sealed record ResultLink(
    string Href,
    string Rel,
    string Method = "GET",
    string? Title = null,
    string? Type = null,
    bool Templated = false)
{
    public static ResultLink Self(string href, string? title = null) => new(href, "self", "GET", title);
    public static ResultLink Related(string href, string relationship, string method = "GET", string? title = null) => new(href, relationship, method, title);
    public static ResultLink Template(string template, string rel, string method = "GET", string? title = null)
        => new(template, rel, method, title, Templated: true);
}
