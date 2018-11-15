using System.Web;
using System.Web.Http.Filters;
using StackExchange.Exceptional;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.API.Filters
{
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is InterviewAccessException)
                return;

            context.Exception.Log(HttpContext.Current);
        }
    }
}
