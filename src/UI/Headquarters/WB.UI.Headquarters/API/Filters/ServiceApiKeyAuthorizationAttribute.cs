using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.API.Filters
{
    [Localizable(false)]
    public class ServiceApiKeyAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IPlainKeyValueStorage<ExportServiceSettings> AppSettingsStorage = //ServiceLocator.Current
                actionContext.Request.GetDependencyScope().GetService(typeof(IPlainKeyValueStorage<ExportServiceSettings>)) as IPlainKeyValueStorage<ExportServiceSettings>;
            var authorizationParameter = actionContext.Request.RequestUri.ParseQueryString()["apikey"];
            var appSetting = AppSettingsStorage.GetById(AppSetting.ExportServiceStorageKey);
            if (appSetting == null)
            {
                throw new ConfigurationErrorsException("Missing required configuration setting");
            }

            if (authorizationParameter != appSetting.Key)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }
    }
}
