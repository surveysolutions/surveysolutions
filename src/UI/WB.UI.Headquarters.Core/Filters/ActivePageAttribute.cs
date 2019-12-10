using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Filters
{
    public class ActivePageAttribute : ResultFilterAttribute
    {
        private readonly MenuItem menuItem;

        public ActivePageAttribute(MenuItem menuItem)
        {
            this.menuItem = menuItem;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var baseController = (Controller)context.Controller;
            baseController.ViewBag.ActivePage = this.menuItem;
            base.OnResultExecuting(context);
        }
    }
}
