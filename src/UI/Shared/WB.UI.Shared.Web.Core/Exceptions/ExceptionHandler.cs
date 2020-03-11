using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Web.Exceptions
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ExceptionHandler(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                logger.Fatal("Unhandled exception", e);
                context.Response.Redirect("/error/500");
            }
        }
    }
}
