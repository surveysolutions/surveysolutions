using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Update")]
    public class UpdateAccountCommand : AccountCommandBase
    {
        public UpdateAccountCommand(Guid accountId, string userName, bool isLockedOut, string passwordQuestion,
            string email, bool isConfirmed, string comment)
            : base(accountId)
        {
            this.UserName = userName;
            this.IsLockedOut = isLockedOut;
            this.PasswordQuestion = passwordQuestion;
            this.Email = email;
            this.IsConfirmed = isConfirmed;
            this.Comment = comment;
        }

        public string UserName { get; private set; }
        public bool IsLockedOut { get; private set; }
        public string PasswordQuestion { get; private set; }
        public string Email { get; private set; }
        public bool IsConfirmed { get; private set; }
        public string Comment { get; private set; }
    }
}