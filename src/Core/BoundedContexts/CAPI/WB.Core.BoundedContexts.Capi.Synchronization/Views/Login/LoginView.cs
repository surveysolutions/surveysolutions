using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Views.Login
{
    public class LoginView
    {
        public LoginView(Guid id, string loginName)
        {
            this.Id = id;
            this.LoginName = loginName;
        }

        public Guid Id { get; private set; }
        public string LoginName { get; private set; }
    }
}
