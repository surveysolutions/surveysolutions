using System;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class RegisterFailedLogin : UserCommand
    {
        public RegisterFailedLogin(Guid userId)
            : base(userId) {}
    }
}
