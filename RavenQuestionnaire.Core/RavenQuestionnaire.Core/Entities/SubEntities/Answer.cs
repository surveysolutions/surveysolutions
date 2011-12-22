using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class Answer : IComposite
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

        public bool Add(IComposite c, Guid? parent)
        {
            return false;
        }

        public bool Remove(IComposite c)
        {
            return false;
        }
        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }
    }
}
