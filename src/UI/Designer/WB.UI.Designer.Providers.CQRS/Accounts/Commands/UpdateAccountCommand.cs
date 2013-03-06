using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "Update")]
    public class UpdateAccountCommand : CommandBase
    {
        public UpdateAccountCommand() {}

        public UpdateAccountCommand(Guid publicKey, string userName, bool isLockedOut, string passwordQuestion,
            string email, bool isConfirmed, string comment)
        {
            PublicKey = publicKey;
            UserName = userName;
            IsLockedOut = isLockedOut;
            PasswordQuestion = passwordQuestion;
            Email = email;
            IsConfirmed = isConfirmed;
            Comment = comment;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string UserName { get; set; }
        public bool IsLockedOut { get; set; }
        public string PasswordQuestion { get; set; }
        public string Email { get; set; }
        public bool IsConfirmed { get; set; }
        public string Comment { get; set; }
    }
}