using System.ComponentModel;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Controllers.Services
{
    [Localizable(false)]
    public class ServiceApiKeyAuthorization : IActionFilter
    {
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;

        public ServiceApiKeyAuthorization(IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings)
        {
            this.exportServiceSettings = exportServiceSettings;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var authorizationParameter = actionContext.HttpContext.Request.Query["apikey"];
            var appSetting = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey);
            if (appSetting == null)
            {
                throw new ConfigurationErrorsException("Missing required configuration setting");
            }

            if (authorizationParameter != appSetting.Key)
            {
                actionContext.Result = new UnauthorizedResult();
            }
        }
    }
}
