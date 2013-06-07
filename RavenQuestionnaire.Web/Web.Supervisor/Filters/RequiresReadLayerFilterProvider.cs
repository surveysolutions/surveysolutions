namespace Web.Supervisor.Filters
{
    using System;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Shared.Web.Filters;

    using Web.Supervisor.Controllers;

    public class RequiresReadLayerFilterProvider : AbstractRequiresReadLayerFilterProvider
    {
        public RequiresReadLayerFilterProvider(IServiceLocator serviceLocator)
            : base(serviceLocator) {}

        protected override Type[] ControllersWhichWorkWithoutReadLayer
        {
            get
            {
                return new[]
                {
                    typeof(MaintenanceController),
                    typeof(ControlPanelController),
                };
            }
        }
    }
}