using System;
using System.Threading;
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

        public bool IsLoggedIn
        {
            get { return RemoteUser != null; }
        }

        public bool LogOn(string userName, string password, CancellationToken cancellationToken)
        {
            if (CapiTesterApplication.DesignerServices.Login(userName, password, cancellationToken))
            {
                RemoteUser = new UserInfo(userName, password);
                currentUser = new UserLight(Guid.NewGuid(), userName);
                return true;
            }
            return false;
        }

        public bool LogOn(string userName, string password)
        {
            return LogOn(userName, password, new CancellationToken());
        }

        public void LogOff()
        {
            RemoteUser = null;
            currentUser = null;
            CapiTesterApplication.Context.ClearAllBackStack<LoginActivity>();
        }
    }
}