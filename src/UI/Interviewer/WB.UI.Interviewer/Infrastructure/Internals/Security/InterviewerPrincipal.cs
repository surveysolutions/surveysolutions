using WB.Core.BoundedContexts.Interviewer;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Interviewer.Infrastructure.Internals.Security
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
                if (this.membership.CurrentUser == null)
                    return null;

                return new InterviewerUserIdentity(this.membership.CurrentUser.Name, this.membership.CurrentUser.Id);
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