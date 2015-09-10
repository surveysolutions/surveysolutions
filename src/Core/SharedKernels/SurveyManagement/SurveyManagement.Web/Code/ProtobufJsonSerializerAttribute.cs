using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public class ProtobufJsonSerializerAttribute : ActionFilterAttribute
    {
        private static readonly ProtobufJsonFormatter protobufJsonFormatter = new ProtobufJsonFormatter();
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            ObjectContent content = actionExecutedContext.Response.Content as ObjectContent;
            if (content == null) return;

            if (content.Formatter is JsonMediaTypeFormatter)
            {
                actionExecutedContext.Response.Content = new ObjectContent(content.ObjectType, content.Value, protobufJsonFormatter);
            }
        }
    }
}