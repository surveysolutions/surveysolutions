using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IAnswer: IComposite
    {
        Guid PublicKey { get; set; }
        string AnswerText { get; set; }
        bool Mandatory { get; set; }
        AnswerType AnswerType { get; set; }
    }

    public class Answer :IAnswer
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

        public void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException("answer is not hierarchical");
        }

        public void Remove(IComposite c)
        {
            throw new CompositeException("answer is not hierarchical");
        }
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            throw new CompositeException("answer is not hierarchical");
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            throw new CompositeException("answer is not hierarchical");
        }
    }
}
