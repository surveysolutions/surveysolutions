using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "ChangePasswordQuestionAndUnswer")]
    public class ChangePasswordQuestionAndAnswerAccountCommand : CommandBase
    {
        public ChangePasswordQuestionAndAnswerAccountCommand() {}

        public ChangePasswordQuestionAndAnswerAccountCommand(Guid publicKey, string passwordQuestion,
            string passwordAnswer)
        {
            PublicKey = publicKey;
            PasswordQuestion = passwordQuestion;
            PasswordAnswer = passwordAnswer;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
    }
}