using System.Web.Mvc;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Filters
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