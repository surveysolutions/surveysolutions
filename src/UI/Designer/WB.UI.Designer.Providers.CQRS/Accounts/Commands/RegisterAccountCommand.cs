using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(AccountAR))]
    public class RegisterAccountCommand : CommandBase
    {
        public RegisterAccountCommand() {}

        public RegisterAccountCommand(string applicationName, string userName, string email, 
                                      object providerUserKey, string password,
                                      string passwordSalt, bool isConfirmed, string confirmationToken)
        {
            ApplicationName = applicationName;
            UserName = userName;
            Email = email;
            ProviderUserKey = providerUserKey;
            Password = password;
            PasswordSalt = passwordSalt;
            IsConfirmed = isConfirmed;
            ConfirmationToken = confirmationToken;
        }

        public string ApplicationName { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public object ProviderUserKey { set; get; }
        public string Password { set; get; }
        public string PasswordSalt { set; get; }
        public bool IsConfirmed { set; get; }
        public string ConfirmationToken { set; get; }
    }
}