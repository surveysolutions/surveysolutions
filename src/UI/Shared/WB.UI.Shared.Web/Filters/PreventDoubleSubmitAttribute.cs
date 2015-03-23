using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using WB.UI.Shared.Web.Resources;

namespace WB.UI.Shared.Web.Filters
{
    public class PreventDoubleSubmitAttribute : ActionFilterAttribute
    {
        private readonly int delayRequest;
        private readonly string errorMessage;

        public PreventDoubleSubmitAttribute(int delayRequest=10, string errorMessage=null)
        {
            this.delayRequest = delayRequest;
            this.errorMessage = string.IsNullOrEmpty(errorMessage)
                ? ErrorMessages.ExcessiveRequestAttemptsDetected
                : errorMessage;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var cache = filterContext.HttpContext.Cache;

            var originationInfo = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress;

            originationInfo += request.UserAgent;
            var targetInfo = request.RawUrl + request.QueryString;

            var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(originationInfo + targetInfo)).Select(s => s.ToString("x2")));

            if (cache[hashValue] != null)
            {
                //Adds the Error Message to the Model and Redirect
                filterContext.Controller.ViewData.ModelState.AddModelError("ExcessiveRequests", errorMessage);
            }
            else
            {
                cache.Add(hashValue, "", null, DateTime.Now.AddSeconds(delayRequest), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }
            base.OnActionExecuting(filterContext);
        }

    }
}
