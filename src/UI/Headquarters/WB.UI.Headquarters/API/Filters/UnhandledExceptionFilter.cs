using System.Web;
using System.Web.Http.Filters;

namespace WB.UI.Headquarters.API.Filters
{
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(context.Exception);
        }
    }
}