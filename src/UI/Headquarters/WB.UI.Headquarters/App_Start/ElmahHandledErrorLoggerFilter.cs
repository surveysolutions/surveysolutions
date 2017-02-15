using System.Web.Http.Filters;
using Elmah;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters
{
    public class ElmahHandledErrorLoggerFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {

            base.OnException(actionExecutedContext);

            if (actionExecutedContext.Exception is WebInterviewAccessException)
                return;

            ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);
        }
    }
}