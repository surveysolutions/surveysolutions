using System;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    public class DesignerAuthentication : IAuthentication
    {
        private readonly IPrincipal principal;

        public DesignerAuthentication(IPrincipal principal)
        {
            this.principal = principal;
        }

        private UserLight currentUser;

        UserLight IAuthentication.CurrentUser
        {
            get { return new UserLight(Guid.NewGuid(), this.principal.CurrentIdentity.Name); }
        }

        public Guid SupervisorId { get; private set; }

        public bool IsLoggedIn
        {
            get { return this.principal.CurrentIdentity.IsAuthenticated; }
        }

        public async Task<bool> LogOnAsync(string userName, string password, CancellationToken cancellationToken)
        {
            throw new Exception("does not supported in current version of questionnaire tester");
        }

        public async Task<bool> LogOnAsync(string userName, string password, bool wasPasswordHashed = false)
        {
            throw new Exception("does not supported in current version of questionnaire tester");
        }

        public void LogOff()
        {
            throw new Exception("does not supported in current version of questionnaire tester");
        }
    }
}
