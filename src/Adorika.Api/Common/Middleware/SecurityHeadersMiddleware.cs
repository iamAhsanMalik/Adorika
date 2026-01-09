namespace Adorika.Api.Common.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "DENY");
        headers.Append("X-XSS-Protection", "1; mode=block");
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        if (!env.IsDevelopment())
        {
            headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await next(context);
    }
}