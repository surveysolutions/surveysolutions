using System.Web;
using System.Web.Http.Controllers;
using WB.UI.Headquarters.Code;
using System.Web.Http.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer;

namespace WB.UI.Headquarters.Filters
{
    public class SupervisorFunctionsEnabledAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (!LegacyOptions.SupervisorFunctionsEnabled)
            {
                var interviewerDevicesController = filterContext.ControllerContext.Controller as InterviewerDevicesController;

                if (interviewerDevicesController != null)
                {
                    throw new HttpException(404, "synchronization controller is missing with current web site configuration");
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}