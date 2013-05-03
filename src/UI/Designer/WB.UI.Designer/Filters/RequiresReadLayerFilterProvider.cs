namespace WB.UI.Designer.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Designer.Controllers;
    using WB.UI.Shared.Web.Filters;

    public class RequiresReadLayerFilterProvider : IFilterProvider
    {
        private readonly IServiceLocator serviceLocator;

        public RequiresReadLayerFilterProvider(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        private static Type[] ControllersWhichWorkWithoutReadLayer
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

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (IsReadLayerRequiredByController(actionDescriptor.ControllerDescriptor))
            {
                yield return new Filter(this.serviceLocator.GetInstance<RequiresReadLayerFilter>(), FilterScope.Controller, order: null);
            }
        }

        private static bool IsReadLayerRequiredByController(ControllerDescriptor controllerDescriptor)
        {
            var isControllerWhichWorksWithoutReadLayer =
                ControllersWhichWorkWithoutReadLayer.Contains(controllerDescriptor.ControllerType);

            return !isControllerWhichWorksWithoutReadLayer;
        }
    }
}