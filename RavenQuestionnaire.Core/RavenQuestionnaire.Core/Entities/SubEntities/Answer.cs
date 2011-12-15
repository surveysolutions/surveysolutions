using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class Answer
    {
        public Answer(/*Question owner*/)
        {
            PublicKey = Guid.NewGuid();
       //     QuestionId = owner.QuestionId;
        }

        public Guid PublicKey { get; set; }
        public string AnswerText { get; set; }
        public bool Mandatory { get; set; }
        public AnswerType AnswerType { get; set; }
       // public string QuestionId { get; set; }
    }
}
