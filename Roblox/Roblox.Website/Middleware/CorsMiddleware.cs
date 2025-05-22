using Microsoft.AspNetCore.Http.Extensions;

namespace Roblox.Website.Middleware;

public class RobloxPlayerCorsMiddleware
{
    private RequestDelegate _next;
    public RobloxPlayerCorsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private string GenerateCspHeader(bool isAuthenticated)
    {
        var connectSrc = "'self' https://*.projex.zip wss://*.projex.zip https://hcaptcha.com https://*.hcaptcha.com https://*.cdn.com https://*.archive.org/* https://web.archive.org https://challenges.cloudflare.com/*";
#if DEBUG
        connectSrc += " ws://localhost:*";
#endif

        // Images
        var imgSrc = "'self' data:";
        if (isAuthenticated)
        {
            imgSrc += " https://*.projex.zip https://*.cdn.com https://*.archive.org http://*.archive.org https://challenges.cloudflare.com/*";
        }
        
        // Scripts
        
        // unsafe-eval required by nextjs
        var scriptSrc =
            "'unsafe-eval' 'self' https://challenges.cloudflare.com/turnstile/v0/api.js https://translate.google.com https://hcaptcha.com https://*.hcaptcha.com https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js https://projex.zip   http://*.archive.org https://*.archive.org http://js.rbxcdn.com/46eace8231bf3c1ce64c55407d9ae60d.js";
        
        return "default-src 'self'; img-src "+imgSrc+"; child-src 'self'; script-src "+scriptSrc+"; frame-src 'self' https://hcaptcha.com https://challenges.cloudflare.com http://challenges.cloudflare.com  https://challenges.cloudflare.com/* http://web.archive.org https://*.archive.org https://web.archive.org/* https://*.hcaptcha.com; style-src 'unsafe-inline' 'self'  http://*.archive.org https://fonts.googleapis.com https://hcaptcha.com https://*.hcaptcha.com https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css ; font-src 'self' fonts.gstatic.com; connect-src "+connectSrc+"; worker-src 'self';";
    }
    
    public async Task InvokeAsync(HttpContext ctx)
    {
        var isAuthenticated = ctx.Items.ContainsKey(".ROBLOSECURITY");
        ctx.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        ctx.Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
        ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Response.Headers["Content-Security-Policy"] = GenerateCspHeader(isAuthenticated);
        await _next(ctx);
    }
}

public static class RobloxPlayerCorsMiddlewareExtensions
{
    public static IApplicationBuilder UseRobloxPlayerCorsMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RobloxPlayerCorsMiddleware>();
    }
}