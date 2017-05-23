using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Filters
{
    public class ActivePageAttribute : ActionFilterAttribute
    {
        private readonly MenuItem activePage;

        public ActivePageAttribute(MenuItem activePage)
        {
            this.activePage = activePage;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.Controller.ViewBag.ActivePage = this.activePage;
            base.OnResultExecuting(filterContext);
        }
    }
}