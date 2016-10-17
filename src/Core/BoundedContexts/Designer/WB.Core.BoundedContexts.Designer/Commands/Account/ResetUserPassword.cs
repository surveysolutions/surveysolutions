using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ResetUserPassword : UserCommand
    {
        public ResetUserPassword(Guid userId, string password, string passwordSalt)
            : base(userId)
        {
            this.Password = password;
            this.PasswordSalt = passwordSalt;
        }

        public string Password { get; private set; }
        public string PasswordSalt { get; private set; }
    }
}