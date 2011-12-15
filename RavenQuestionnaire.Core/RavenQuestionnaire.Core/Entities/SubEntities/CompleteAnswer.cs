using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class CompleteAnswer
    {
        public CompleteAnswer()
        {
        }
        public CompleteAnswer(Answer answer)
        {
            this.PublicKey = answer.PublicKey;
         //   this.CustomAnswer = answer.AnswerText;
        }
        public Guid PublicKey { get; set; }
        public string CustomAnswer { get; set; }
    }
}
