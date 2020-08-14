using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.UI.Headquarters.Services.Impl
{
    public class DesignerUserCredentials : IDesignerUserCredentials
    {
        private readonly IHttpContextAccessor contextAccessor;

        // DesignerUserCredentials is scoped service. current will contains credentials per scope.
        private RestCredentials current = null;

        public DesignerUserCredentials(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public virtual RestCredentials Get()
        {
            if (current != null) return current;

            var restCredentials = contextAccessor.HttpContext?.Session.GetString("designerCredentials");
            if (restCredentials == null) return null;

            current = JsonConvert.DeserializeObject<RestCredentials>(restCredentials);
            return current;
        }

        public void Set(RestCredentials credentials)
        {
            current = credentials;
            contextAccessor.HttpContext.Session.SetString("designerCredentials", JsonConvert.SerializeObject(credentials));
        }
    }
}
