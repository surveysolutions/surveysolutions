using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.API.Filters
{
    [Localizable(false)]
    public class ServiceApiKeyAuthorizationAttribute : ActionFilterAttribute
    {
        [Inject]
        public IPlainKeyValueStorage<ExportServiceSettings> AppSettingsStorage { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
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
