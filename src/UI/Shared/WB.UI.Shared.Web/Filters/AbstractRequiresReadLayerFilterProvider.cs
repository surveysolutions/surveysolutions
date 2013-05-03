namespace WB.UI.Shared.Web.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.Practices.ServiceLocation;

    public abstract class AbstractRequiresReadLayerFilterProvider : IFilterProvider
    {
        private readonly IServiceLocator serviceLocator;

        public AbstractRequiresReadLayerFilterProvider(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        protected abstract Type[] ControllersWhichWorkWithoutReadLayer { get; }

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (this.IsReadLayerRequiredByController(actionDescriptor.ControllerDescriptor))
            {
                yield return new Filter(this.serviceLocator.GetInstance<RequiresReadLayerFilter>(), FilterScope.Controller, order: null);
            }
        }

        private bool IsReadLayerRequiredByController(ControllerDescriptor controllerDescriptor)
        {
            var isControllerWhichWorksWithoutReadLayer =
                this.ControllersWhichWorkWithoutReadLayer.Contains(controllerDescriptor.ControllerType);

            return !isControllerWhichWorksWithoutReadLayer;
        }
    }
}