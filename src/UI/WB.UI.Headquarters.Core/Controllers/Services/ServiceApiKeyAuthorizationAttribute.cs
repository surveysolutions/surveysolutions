using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Controllers.Services
{
    public class ServiceApiKeyAuthorizationAttribute : ServiceFilterAttribute
    {
        public ServiceApiKeyAuthorizationAttribute() : base(typeof(ServiceApiKeyAuthorization))
        {
            
        }
    }
}
