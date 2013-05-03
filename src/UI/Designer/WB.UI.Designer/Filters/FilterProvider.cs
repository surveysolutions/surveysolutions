namespace WB.UI.Designer.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Designer.Controllers;

    public class FilterProvider : IFilterProvider
    {
        private readonly IServiceLocator serviceLocator;

        public FilterProvider(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            bool isErrorController = IsControllerOfType<ErrorController>(actionDescriptor.ControllerDescriptor);
            bool isMaintenanceController = IsControllerOfType<MaintenanceController>(actionDescriptor.ControllerDescriptor);

            bool isReadLayerRequiredByController = !isErrorController && !isMaintenanceController;

            if (isReadLayerRequiredByController)
            {
                yield return new Filter(this.serviceLocator.GetInstance<RequiresReadLayerFilter>(), FilterScope.Controller, order: null);
            }
        }

        private static bool IsControllerOfType<T>(ControllerDescriptor controllerDescriptor)
        {
            return controllerDescriptor.ControllerType == typeof(T);
        }
    }
}