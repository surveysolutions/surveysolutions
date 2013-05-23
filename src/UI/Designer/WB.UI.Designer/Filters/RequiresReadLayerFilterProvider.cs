namespace WB.UI.Designer.Filters
{
    using System;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Designer.Controllers;
    using WB.UI.Shared.Web.Filters;

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
                    typeof(ErrorController),
                    typeof(MaintenanceController),
                };
            }
        }
    }
}