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
        public static readonly JsonSerializerSettings PublicApiSerializerSettings =
            new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                    },
                    Converters = {new EnumToStringConverter()}
                };
                
        

        public override void OnActionExecuted(ActionExecutedContext ctx)
        {
            if (ctx.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Add(new NewtonsoftJsonOutputFormatter(
                    PublicApiSerializerSettings,
                    ctx.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(),
                    ctx.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value, null));
            }
        }
    }
}
