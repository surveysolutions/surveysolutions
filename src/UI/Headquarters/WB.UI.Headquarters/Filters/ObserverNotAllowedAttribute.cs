﻿using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        private IAuthorizedUser authorizedUser => ServiceLocator.Current.GetInstance<IAuthorizedUser>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (this.authorizedUser.IsObserver)
                filterContext.Result = new HttpForbiddenResult(Strings.ObserverNotAllowed);
            else
                base.OnActionExecuting(filterContext);
        }
    }
}