using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.UI.Headquarters.Services.Impl
{
    public class DesignerUserCredentials : IDesignerUserCredentials
    {
        private readonly IHttpContextAccessor contextAccessor;

        public DesignerUserCredentials(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public virtual RestCredentials Get()
        {
            var restCredentials = contextAccessor.HttpContext.Session.GetString("designerCredentials");
            if (restCredentials != null)
            {
                return JsonConvert.DeserializeObject<RestCredentials>(restCredentials);
            }

            return null;
        }

        public void Set(RestCredentials credentials)
        {
            contextAccessor.HttpContext.Session.SetString("designerCredentials", JsonConvert.SerializeObject(credentials));
        }
    }
}
