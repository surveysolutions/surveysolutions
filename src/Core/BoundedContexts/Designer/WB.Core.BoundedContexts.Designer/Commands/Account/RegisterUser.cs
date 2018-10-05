using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    public class RegisterUser : UserCommand
    {
        public RegisterUser(string applicationName, string userName, string email,
            Guid userId, string password,
            string passwordSalt, bool isConfirmed, string confirmationToken, string fullName)
            : base(userId)
        {
            this.ApplicationName = applicationName;
            this.UserName = userName;
            this.Email = email;
            this.Password = password;
            this.PasswordSalt = passwordSalt;
            this.IsConfirmed = isConfirmed;
            this.ConfirmationToken = confirmationToken;
            this.FullName = fullName;
        }

        public string FullName { get; }

        public string ApplicationName { private set; get; }
        public string UserName { private set; get; }
        public string Email { private set; get; }
        public string Password { private set; get; }
        public string PasswordSalt { private set; get; }
        public bool IsConfirmed { private set; get; }
        public string ConfirmationToken { private set; get; }
    }
}
