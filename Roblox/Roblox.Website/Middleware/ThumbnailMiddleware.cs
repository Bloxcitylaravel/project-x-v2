using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Roblox.Website.Middleware
{
    public class ThumbnailMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _basePath;

        public ThumbnailMiddleware(RequestDelegate next, string basePath)
        {
            _next = next;
            _basePath = Path.GetFullPath(basePath);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;

            var normalizedPath = NormalizePath(requestPath, "/images/thumbnails");

            if (!string.IsNullOrEmpty(normalizedPath))
            {
                var relativePath = normalizedPath.Substring("/images/thumbnails".Length).TrimStart('/');
                var filePathWithoutExtension = Path.Combine(_basePath, relativePath);

                filePathWithoutExtension = Path.GetFullPath(filePathWithoutExtension);
                if (!filePathWithoutExtension.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

                if (File.Exists(filePathWithoutExtension))
                {
                    context.Response.ContentType = "image/png";
                    await context.Response.SendFileAsync(filePathWithoutExtension);
                    return;
                }

                var defaultPngPath = Path.ChangeExtension(filePathWithoutExtension, ".png");
                if (File.Exists(defaultPngPath))
                {
                    context.Response.ContentType = "image/png";
                    await context.Response.SendFileAsync(defaultPngPath);
                    return;
                }
            }

            await _next(context);
        }

        private string NormalizePath(string requestPath, string basePath)
        {
            while (requestPath.StartsWith(basePath + basePath, StringComparison.OrdinalIgnoreCase))
            {
                requestPath = requestPath.Substring(basePath.Length);
            }

            if (requestPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return requestPath;
            }

            return string.Empty;
        }
    }
}
