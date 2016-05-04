using System.Web.Http.Controllers;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Filters
{
    public class ApiMaintenanceFilter : AbstractApiMaintenanceFilter
    {
        protected override bool SkipControllerToCheck(IHttpController controller)
        {
            return controller is ControlPanelApiController;
        }
    }
}