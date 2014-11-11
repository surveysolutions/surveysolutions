using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
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