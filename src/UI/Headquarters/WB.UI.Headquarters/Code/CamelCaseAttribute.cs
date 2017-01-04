using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;
using Newtonsoft.Json.Serialization;

namespace WB.UI.Headquarters.Code
{
    public class CamelCaseAttribute : ActionFilterAttribute
    {
        private static readonly JsonMediaTypeFormatter CamelCasingFormatter = new JsonMediaTypeFormatter();

        static CamelCaseAttribute()
        {
            CamelCasingFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            CamelCasingFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            ObjectContent content = actionExecutedContext.Response?.Content as ObjectContent;
            if (content!=null)
            {
                actionExecutedContext.Response.Content = new ObjectContent(content.ObjectType, content.Value, CamelCasingFormatter);
            }
        }
    }
}