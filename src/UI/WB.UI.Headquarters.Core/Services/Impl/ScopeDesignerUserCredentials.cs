using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.UI.Headquarters.Services.Impl
{
    public class ScopeDesignerUserCredentials : IDesignerUserCredentials
    {
        private RestCredentials credentials;

        public virtual RestCredentials Get() => credentials;

        public void Set(RestCredentials credentials)
        {
            this.credentials = credentials;
        }
    }
}
