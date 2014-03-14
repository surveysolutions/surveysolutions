using System;
using System.Web.Mvc;
using Machine.Specifications;
using Machine.Specifications.Annotations;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.Controllers
{
    public static class MvcExtensions
    {
        public static T GetModel<T>([NotNull] this ActionResult actionResult) where T : class
        {
            if (actionResult == null) throw new ArgumentNullException("actionResult");

            var viewResult = actionResult as ViewResult;
            var model = viewResult.Model as T;
            return model;
        }

        public static void ShouldBeRedirectToAction(this ActionResult actionResult, string action)
        {
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();
            var redirectToRouteResult = (RedirectToRouteResult) actionResult;
            redirectToRouteResult.RouteValues["action"].ShouldEqual(action);
        }
    }
}