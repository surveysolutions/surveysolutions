using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IAnswer: IComposite
    {
        object AnswerValue { get; set; }
        string AnswerText { get; set; }
        bool Mandatory { get; set; }
        AnswerType AnswerType { get; set; }
    }

    public class Answer :IAnswer
    {
        public Answer(/*Question owner*/)
        {
            PublicKey = Guid.NewGuid();
            this.observers=new List<IObserver<CompositeEventArgs>>();
       //     QuestionId = owner.QuestionId;
        }

        public Guid PublicKey { get; set; }
        public string AnswerText { get; set; }
        public bool Mandatory { get; set; }
        public object AnswerValue { get; set; }
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

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            throw new CompositeException("answer is not hierarchical");
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            throw new CompositeException("answer is not hierarchical");
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber<CompositeEventArgs>(observers, observer);
        }
        private List<IObserver<CompositeEventArgs>> observers;
        #endregion

    }
}
