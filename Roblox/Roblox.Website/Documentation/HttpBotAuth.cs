using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;

namespace Roblox.Website.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class BotAuthorizationAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new BotAuthorizationFilter();
        }
    }

    public class BotAuthorizationFilter : IActionFilter
    {

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("PJX-BOTAUTH", out StringValues botKey))
            {
                if (botKey != Configuration.BotAuthorization)
                {
                    Console.WriteLine("Failed auth");
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
            }
            else
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
