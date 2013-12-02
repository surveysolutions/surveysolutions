using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
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
            this.ApplicationName = applicationName;
            this.UserName = userName;
            this.Email = email;
            this.AccountId = accountId;
            this.Password = password;
            this.PasswordSalt = passwordSalt;
            this.IsConfirmed = isConfirmed;
            this.ConfirmationToken = confirmationToken;
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