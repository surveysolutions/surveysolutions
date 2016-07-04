﻿using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class ObserverNotAllowedApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (HttpContext.Current.User.Identity.IsObserver())
            {
                filterContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    Content = new StringContent(Strings.ObserverNotAllowed)
                };
            }
            else
                base.OnActionExecuting(filterContext);
        }
    }

    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (HttpContext.Current.User.Identity.IsObserver())
            {
                throw new HttpException(403, Strings.ObserverNotAllowed);
            }

            base.OnActionExecuting(actionContext);
        }
    }
}