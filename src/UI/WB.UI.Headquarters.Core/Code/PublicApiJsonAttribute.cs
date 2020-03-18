using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Code
{
    public class PublicApiJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext ctx)
        {
            if (ctx.Result is ObjectResult objectResult)
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                    }
                };
                jsonSerializerSettings.Converters.Add(new EnumToStringConverter());
                objectResult.Formatters.Add(new NewtonsoftJsonOutputFormatter(
                    jsonSerializerSettings,
                    ctx.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(),
                    ctx.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value));
            }
        }
    }
}
