using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof (AccountAR))]
    public class RegisterAccountCommand : CommandBase
    {
        public RegisterAccountCommand(string applicationName, string userName, string email,
            Guid accountId, string password,
            string passwordSalt, bool isConfirmed, string confirmationToken)
            : base(accountId)
        {
            ApplicationName = applicationName;
            UserName = userName;
            Email = email;
            AccountId = accountId;
            Password = password;
            PasswordSalt = passwordSalt;
            IsConfirmed = isConfirmed;
            ConfirmationToken = confirmationToken;
        }

        public string ApplicationName { private set; get; }
        public string UserName { private set; get; }
        public string Email { private set; get; }
        public Guid AccountId { private set; get; }
        public string Password { private set; get; }
        public string PasswordSalt { private set; get; }
        public bool IsConfirmed { private set; get; }
        public string ConfirmationToken { private set; get; }
    }
}