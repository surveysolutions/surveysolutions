using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        private IIdentityManager identityManager => ServiceLocator.Current.GetInstance<IIdentityManager>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (this.identityManager.IsCurrentUserObserver)
                filterContext.Result = new HttpForbiddenResult(Strings.ObserverNotAllowed);
            else
                base.OnActionExecuting(filterContext);
        }
    }
}