using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Roblox.Website.Lib;

namespace Roblox.Website.Middleware;

public class RobloxLoggingMiddleware
{
    private RequestDelegate _next;
    public RobloxLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext ctx)
    {
        string userAgent = ctx.Request.Headers["User-Agent"].ToString();
        var watch = new Stopwatch();
        watch.Start();
        await _next(ctx);
        watch.Stop();

        var str = $"[{ctx.Request.Method.ToUpper()}] {ctx.Request.GetEncodedUrl()} - {watch.ElapsedMilliseconds}ms";
        if(ctx.Request.GetEncodedUrl().Contains("e.png"))
        {
            return;
        }
        if(!ctx.Request.GetEncodedUrl().Contains("voice"))
        {
            return;
        }
        Console.WriteLine(str);
    }
}

public static class RobloxLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRobloxLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RobloxLoggingMiddleware>();
    }
}