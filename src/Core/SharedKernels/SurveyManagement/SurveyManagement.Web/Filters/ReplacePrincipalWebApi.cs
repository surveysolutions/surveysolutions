using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class ReplacePrincipalWebApi : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            PrincipalReplacer.ReplacePrincipal();
            base.OnActionExecuting(filterContext);
        }
    }
}