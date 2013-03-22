namespace WB.UI.Designer.Filters
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using WB.UI.Designer.Controllers;

    public class FilterProvider : IFilterProvider
    {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            bool isErrorController = actionDescriptor.ControllerDescriptor.ControllerType == typeof(ErrorController);
            bool isReadLayerRequiredByController = !isErrorController;

            if (isReadLayerRequiredByController)
            {
                #warning TLK: do not instantiate filter directly, replace with service locator call
                yield return new Filter(new RequiresReadLayerFilter(), FilterScope.Controller, order: null);
            }
        }
    }
}