using System.Web.Http.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class ApiMaintenanceFilter : AbstractApiMaintenanceFilter
    {
        protected override bool SkipControllerToCheck(IHttpController controller)
        {
            return controller is ControlPanelApiController;
        }
    }
}