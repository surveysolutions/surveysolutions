using System;

namespace WB.Core.BoundedContexts.Capi.Views.Login
{
    public class LoginViewInput
    {
        public LoginViewInput(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }
    }
}
