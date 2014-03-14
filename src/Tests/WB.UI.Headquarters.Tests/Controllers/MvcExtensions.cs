using System;
using System.Web.Mvc;
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
    }
}