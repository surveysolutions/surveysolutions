using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Serialization;

namespace WB.UI.Shared.Web.Attributes
{
    public class CamelCaseAttribute : ActionFilterAttribute
    {
        private static readonly JsonMediaTypeFormatter CamelCasingFormatter = new JsonMediaTypeFormatter();

        static CamelCaseAttribute()
        {
            CamelCasingFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            CamelCasingFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            ObjectContent content = context.Response?.Content as ObjectContent;
            if (content != null)
            {
                context.Response.Content = new ObjectContent(content.ObjectType, content.Value, CamelCasingFormatter);
            }
        }
    }
}
