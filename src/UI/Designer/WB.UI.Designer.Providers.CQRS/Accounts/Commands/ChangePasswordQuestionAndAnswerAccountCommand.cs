using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangePasswordQuestionAndUnswer")]
    public class ChangePasswordQuestionAndAnswerAccountCommand : AccountCommandBase
    {
        public ChangePasswordQuestionAndAnswerAccountCommand(Guid accountId, string passwordQuestion,
            string passwordAnswer)
            : base(accountId)
        {
            PasswordQuestion = passwordQuestion;
            PasswordAnswer = passwordAnswer;
        }

        public string PasswordQuestion { get; private set; }
        public string PasswordAnswer { get; private set; }
    }
}