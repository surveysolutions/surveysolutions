using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    /// <summary>
    /// Filter that allows to produce swagger docs only for controllers from one namespace
    /// </summary>
    public class OnlyPublicApiConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            action.ApiExplorer.IsVisible = 
                action.Controller?.ControllerType?.Namespace?.Contains("PublicApi") == true;
        }
    }

}
