using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangePasswordQuestionAndUnswer")]
    public class ChangePasswordQuestionAndAnswerAccountCommand : AccountCommandBase
    {
        public ChangePasswordQuestionAndAnswerAccountCommand(Guid accountId, string passwordQuestion,
            string passwordAnswer)
            : base(accountId)
        {
            this.PasswordQuestion = passwordQuestion;
            this.PasswordAnswer = passwordAnswer;
        }

        public string PasswordQuestion { get; private set; }
        public string PasswordAnswer { get; private set; }
    }
}