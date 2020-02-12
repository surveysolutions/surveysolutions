using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Designer
{
    public class ScopeDesignerUserCredentials : IDesignerUserCredentials
    {
        private RestCredentials credentials;

        public ScopeDesignerUserCredentials()
        {
        }

        public ScopeDesignerUserCredentials(RestCredentials credentials)
        {
            this.credentials = credentials;
        }

        public virtual RestCredentials Get() => credentials;

        public void Set(RestCredentials credentials)
        {
            this.credentials = credentials;
        }
    }
}
