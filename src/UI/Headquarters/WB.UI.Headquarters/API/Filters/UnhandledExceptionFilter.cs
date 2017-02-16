using System.Web;
using System.Web.Http.Filters;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.Filters
{
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is WebInterviewAccessException)
                return;

            Elmah.ErrorSignal.FromCurrentContext().Raise(context.Exception);
        }
    }
}