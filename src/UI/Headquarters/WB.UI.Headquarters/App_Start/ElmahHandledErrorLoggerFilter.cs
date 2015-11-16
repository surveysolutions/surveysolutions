using System.Web;
using System.Web.Http.Filters;
using Elmah;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;

namespace WB.UI.Headquarters
{
    public class ElmahHandledErrorLoggerFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            if (actionExecutedContext.Exception.IsHttpNotFound()) return;

            ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);
        }
    }
}