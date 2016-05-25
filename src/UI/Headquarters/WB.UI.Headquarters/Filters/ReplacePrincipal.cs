using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class ReplacePrincipal : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            PrincipalReplacer.ReplacePrincipal();
            base.OnActionExecuting(filterContext);
        }
    }
}