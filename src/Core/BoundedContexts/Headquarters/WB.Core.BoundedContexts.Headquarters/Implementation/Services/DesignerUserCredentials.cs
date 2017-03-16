using System.Web;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class DesignerUserCredentials
    {
        private readonly IIdentityManager identityManager;

        public DesignerUserCredentials() { }
        public DesignerUserCredentials(IIdentityManager identityManager)
        {
            this.identityManager = identityManager;
        }

        public virtual RestCredentials Get()
            => (RestCredentials)HttpContext.Current.Session[this.identityManager.CurrentUserName];

        public void Set(RestCredentials credentials)
            => HttpContext.Current.Session[this.identityManager.CurrentUserName] = credentials;
    }
}