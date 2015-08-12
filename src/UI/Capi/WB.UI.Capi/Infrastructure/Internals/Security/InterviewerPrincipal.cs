using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Capi.Infrastructure.Internals.Security
{
    public class InterviewerPrincipal : IPrincipal
    {
        readonly IAuthentication membership;

        public InterviewerPrincipal(IAuthentication membership)
        {
            this.membership = membership;
        }

        public bool IsAuthenticated
        {
            get { return this.membership.IsLoggedIn; }
        }

        public IUserIdentity CurrentUserIdentity
        {
            get
            {
                UserLight user = this.membership.CurrentUser;
                if (user == null) return null;
                var identity = new InterviewerUserIdentity(user.Name, user.Id);
                return identity;
            }
        }

        public void SignOut()
        {
            this.membership.LogOff();
        }

        public void SignIn(string userName, string password, bool rememberMe)
        {
            this.membership.LogOnAsync(userName, password);
        }
    }
}