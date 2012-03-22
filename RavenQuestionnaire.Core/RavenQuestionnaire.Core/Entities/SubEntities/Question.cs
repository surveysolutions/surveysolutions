using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IQuestion : IComposite, ITriggerable
    {
        string QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        string ConditionExpression { get; set; }
        string StataExportCaption { get; set; }
        string Instructions { get; set; }
        List<Image> Cards { get; set; }
    }

    public interface IQuestion<T> : IQuestion where T : IAnswer
    {
        List<T> Answers { get; set; }
    }

    public class Question : /*IEntity<QuestionDocument>*/IQuestion<IAnswer>
    {

        public Question()
        {
            PublicKey = Guid.NewGuid();
            Answers = new List<IAnswer>();
            Cards = new List<Image>();
            this.Triggers=new List<Guid>();
            this.observers = new List<IObserver<CompositeEventArgs>>();

        }

        public Question(string text, QuestionType type)
            : this()
        {
            QuestionText = text;
            QuestionType = type;
        }
        protected void OnAdded(CompositeAddedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers)
            {
                observer.OnNext(e);
            }
        }
        protected void OnRemoved(CompositeRemovedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers)
            {
                observer.OnNext(e);
            }
        } 
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<IAnswer> Answers { get; set; }
        public List<Image> Cards { get; set; }

        public string ConditionExpression
        {
            get { return this.conditionExpression; }
            set
            {
                this.conditionExpression = value;
                QuestionnaireParametersParser parser = new QuestionnaireParametersParser();
                this.Triggers = parser.Execute(value);
            }
        }

        private string conditionExpression;
        //remove when exportSchema will be done 
        public string StataExportCaption { get; set; }

        public string Instructions{ get; set; }

        public void ClearAnswers()
        {
            Answers.Clear();
        }

        public void UpdateAnswerList(IEnumerable<Answer> answers)
        {
            ClearAnswers();
            foreach (Answer answer in answers)
            {
                Add(answer, PublicKey);
            }
        }

        public void AddCard(Image card)
        {
            if (Cards == null)
                Cards = new List<Image>();
            Cards.Add(card);
            //OnAdded(new CompositeAddedEventArgs(card));
        }
        public void AddAnswer(IAnswer answer)
        {
            if (Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
                throw new DuplicateNameException("answer with current publick key already exist");
            Answers.Add(answer);
            OnAdded(new CompositeAddedEventArgs(answer));
        }

        public void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                IAnswer answer = c as IAnswer;
                if (answer != null)
                {
                    AddAnswer(answer);
                    return;
                }
            }
            throw new CompositeException();
        }

        public void Remove(IComposite c)
        {
            IAnswer answer = c as IAnswer;
            if (answer != null)
            {
                Answers.Remove(answer);
                OnRemoved(new CompositeRemovedEventArgs(answer));
                return;
            }
            throw new CompositeException();
        }
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T).IsAssignableFrom(typeof(IAnswer)))
            {
                if (Answers.RemoveAll(a => a.PublicKey.Equals(publicKey)) > 0)
                {
                    return;
                }
            }
            throw new CompositeException();
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T).IsAssignableFrom(typeof(IAnswer)))
                return Answers.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {

            return Answers.Where(a => a is T && condition(a as T)).Select(a => a as T);
            /* if (typeof(T) == typeof(Answer))
                 return Answers.Where(a => condition(a)).Select(a => a as T);
             return null;*/
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return Answers.Where(a => a is T && condition(a as T)).Select(a => a as T).FirstOrDefault();
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

        #region Implementation of ITriggerable

        public List<Guid> Triggers { get; set; }

        #endregion


    }
}
