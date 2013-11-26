using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Update")]
    public class UpdateAccountCommand : AccountCommandBase
    {
        public UpdateAccountCommand(Guid accountId, string userName, bool isLockedOut, string passwordQuestion,
            string email, bool isConfirmed, string comment)
            : base(accountId)
        {
            UserName = userName;
            IsLockedOut = isLockedOut;
            PasswordQuestion = passwordQuestion;
            Email = email;
            IsConfirmed = isConfirmed;
            Comment = comment;
        }

        public string UserName { get; private set; }
        public bool IsLockedOut { get; private set; }
        public string PasswordQuestion { get; private set; }
        public string Email { get; private set; }
        public bool IsConfirmed { get; private set; }
        public string Comment { get; private set; }
    }
}