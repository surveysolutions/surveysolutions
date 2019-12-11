using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    public class OnlyPublicApiConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            action.ApiExplorer.IsVisible = 
                action.Controller?.ControllerType?.Namespace?.Contains("PublicApi") == true;
        }
    }

}
