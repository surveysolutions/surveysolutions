using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
/*using System.Web.Http.Controllers;
using System.Web.Http.Filters;*/
using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class SupervisorFunctionsEnabledAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!LegacyOptions.SupervisorFunctionsEnabled)
            {
                if (filterContext.Controller is SyncController)
                {
                    throw new HttpException(404, "synchronization controller is missing with current web site configuration");
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}