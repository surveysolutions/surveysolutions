using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ChangeUserPassword : UserCommand
    {
        public ChangeUserPassword(Guid userId, string password)
            : base(userId)
        {
            this.Password = password;
        }

        public string Password { get; private set; }
    }
}