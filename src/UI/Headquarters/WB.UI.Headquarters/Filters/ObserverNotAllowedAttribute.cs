using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        private IAuthorizedUser authorizedUser => DependencyResolver.Current.GetService<IAuthorizedUser>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (this.authorizedUser.IsObserver)
                filterContext.Result = new HttpForbiddenResult(Strings.ObserverNotAllowed);
            else
                base.OnActionExecuting(filterContext);
        }
    }
}
