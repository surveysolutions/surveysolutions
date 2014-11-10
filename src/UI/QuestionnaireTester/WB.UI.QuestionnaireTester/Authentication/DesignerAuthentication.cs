using System;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.QuestionnaireTester.Authentication
{
    public class DesignerAuthentication  : IAuthentication
    {
        public UserInfo RemoteUser { get; private set; }

        private UserLight currentUser; 
    
        
        UserLight IAuthentication.CurrentUser {
            get { return currentUser; }
        }

        public Guid SupervisorId { get; private set; }

        public bool IsLoggedIn
        {
            get { return RemoteUser != null; }
        }

        public async Task<bool> LogOn(string userName, string password, CancellationToken cancellationToken)
        {
            var loggedIn = await CapiTesterApplication.DesignerServices.Login(userName, password, cancellationToken);
            if (loggedIn)
            {
                RemoteUser = new UserInfo(userName, password);
                currentUser = new UserLight(Guid.NewGuid(), userName);
                return true;
            }
            return false;
        }

        public async Task<bool> LogOn(string userName, string password, bool wasPasswordHashed = false)
        {
            return await LogOn(userName, password, new CancellationToken());
        }

        public void LogOff()
        {
            RemoteUser = null;
            currentUser = null;
            CapiTesterApplication.Context.ClearAllBackStack<LoginActivity>();
        }
    }
}