using System.Web;
using System.Web.Http.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.UI.Headquarters.Code;
using System.Web.Http.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class SupervisorFunctionsEnabledAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (!LegacyOptions.SupervisorFunctionsEnabled)
            {
                var controller = filterContext.ControllerContext.Controller as InterviewerSyncController;

                if (controller != null)
                {
                    throw new HttpException(404, "synchronization controller is missing with current web site configuration");
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}