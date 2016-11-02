using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ChangeSecurityQuestion : UserCommand
    {
        public ChangeSecurityQuestion(Guid userId, string passwordQuestion,
            string passwordAnswer)
            : base(userId)
        {
            this.PasswordQuestion = passwordQuestion;
            this.PasswordAnswer = passwordAnswer;
        }

        public string PasswordQuestion { get; private set; }
        public string PasswordAnswer { get; private set; }
    }
}