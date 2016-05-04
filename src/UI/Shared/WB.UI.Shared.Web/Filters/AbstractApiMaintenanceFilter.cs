using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net.Http;

namespace WB.UI.Shared.Web.Filters
{
    public abstract class AbstractApiMaintenanceFilter : ActionFilterAttribute
    {
        private IReadSideStatusService ReadSideStatusService => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (!SkipControllerToCheck(filterContext.ControllerContext.Controller))
            {
                if (this.ReadSideStatusService.AreViewsBeingRebuiltNow() ||
                    this.ReadSideStatusService.IsReadSideOutdated())
                {
                    filterContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.NotAcceptable)
                    {
                        Content = new StringContent("Server is in maintenance mode.")
                    };
                }
            }
            base.OnActionExecuting(filterContext);
        }

        protected abstract bool SkipControllerToCheck(IHttpController controller);
    }
}