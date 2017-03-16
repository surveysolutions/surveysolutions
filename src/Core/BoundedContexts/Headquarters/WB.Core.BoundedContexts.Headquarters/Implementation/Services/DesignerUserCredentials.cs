using System.Web;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class DesignerUserCredentials
    {
        private readonly IAuthorizedUser authorizedUser;

        public DesignerUserCredentials() { }
        public DesignerUserCredentials(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser;
        }

        public virtual RestCredentials Get()
            => (RestCredentials)HttpContext.Current.Session[this.authorizedUser.UserName];

        public void Set(RestCredentials credentials)
            => HttpContext.Current.Session[this.authorizedUser.UserName] = credentials;
    }
}