using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Shared.Web.Filters;
using ControlPanelController = WB.UI.Headquarters.Controllers.ControlPanelController;

namespace WB.UI.Headquarters.Filters
{
    public class MaintenanceFilter : AbstractMaintenanceFilter
    {
        protected override bool IsMaintenanceController(ControllerBase controller)
        {
            return controller is MaintenanceController;
        }

        protected override bool IsControlPanelController(ControllerBase controller)
        {
            return controller is ControlPanelController;
        }
    }
}