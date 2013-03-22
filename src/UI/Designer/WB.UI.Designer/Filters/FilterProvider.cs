namespace WB.UI.Designer.Filters
{
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
            bool isErrorController = actionDescriptor.ControllerDescriptor.ControllerType == typeof(ErrorController);
            bool isReadLayerRequiredByController = !isErrorController;

            if (isReadLayerRequiredByController)
            {
                yield return new Filter(this.serviceLocator.GetInstance<RequiresReadLayerFilter>(), FilterScope.Controller, order: null);
            }
        }
    }
}