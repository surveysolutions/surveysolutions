using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Utility.OrderStrategy;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteQuestion : ICompleteQuestion
    {
        public CompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            this.Valid = true;
            this.Children = new List<IComposite>();
            this.Cards = new List<Image>();
            this.Triggers = new List<Guid>();
            this.Attributes = new Dictionary<string, object>();
            //this.Answers.GetObservablePropertyChanges().Subscribe(e=>e.EventArgs)
            this.observers = new List<IObserver<CompositeEventArgs>>();
        }

        public CompleteQuestion(string text, QuestionType type)
            : this()
        {

            this.QuestionText = text;
            this.QuestionType = type;
        }
        public static explicit operator CompleteQuestion(RavenQuestionnaire.Core.Entities.SubEntities.Question doc)
        {
            CompleteQuestion result = new CompleteQuestion
                                          {
                                              PublicKey = doc.PublicKey,
                                              ConditionExpression = doc.ConditionExpression,
                                              QuestionText = doc.QuestionText,
                                              QuestionType = doc.QuestionType,
                                              StataExportCaption = doc.StataExportCaption,
                                              Instructions = doc.Instructions,
                                              Triggers = doc.Triggers,
                                              ValidationExpression = doc.ValidationExpression,
                                              AnswerOrder = doc.AnswerOrder,
                                              Valid = true,
                                              Featured = doc.Featured,
                                              Attributes = doc.Attributes
                                          };
            var ansersToCopy = new OrderStrategyFactory().Get(result.AnswerOrder).Reorder(doc.Children);
            new CompleteQuestionFactory().Create(result).Create(ansersToCopy);
         /*   foreach (IAnswer answer in ansersToCopy)
            {
                var newanswer = new CompleteAnswerFactory().ConvertToCompleteAnswer(answer);
                result.Children.Add(newanswer);
                result.OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(result), newanswer));
            }*/
            if (doc.Cards != null)
                foreach (var card in doc.Cards)
                {
                    result.Cards.Add(card);
                }
            return result;
        }
        public Guid PublicKey { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public string ConditionExpression { get; set; }

        public string ValidationExpression
        { get; set; }

        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public List<Image> Cards { get; set; }

        public Order AnswerOrder { get; set; }

        public bool Featured { get; set; }

        public Dictionary<string, object> Attributes { get; set; }

        public bool Enabled { get; set; }

        public bool Valid { get; set; }

        public DateTime? AnswerDate { get; set; }

        public void Add(IComposite c, Guid? parent)
        {
            new CompleteQuestionFactory().Create(this).Add(c, parent);
            this.AnswerDate = DateTime.Now;
            OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), c));
        }

        public void Remove(IComposite c)
        {
            this.Remove(c.PublicKey);
        }

        public void Remove(Guid publicKey)
        {
            if (this.PublicKey == publicKey)
            {
                new CompleteQuestionFactory().Create(this).Remove();
                OnRemoved(new CompositeRemovedEventArgs(this));
                return;
            }

            foreach (CompleteAnswer completeAnswer in this.Children)
            {
                try
                {
                    completeAnswer.Remove(publicKey);
                    OnRemoved(new CompositeRemovedEventArgs(new CompositeRemovedEventArgs(this), completeAnswer));
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException("answer wasn't found");
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T).IsAssignableFrom(GetType()))
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            if (typeof(T).IsAssignableFrom(typeof(CompleteAnswer)))
            {
                return this.Children.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            }
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                Children.Where(a => a is T && condition(a as T)).Select
                    (a => a as T);
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return Find<T>(condition).FirstOrDefault();
        }

        public List<IComposite> Children { get; set; }
        [JsonIgnore]
        public IComposite Parent
        {
            get { throw new NotImplementedException(); }
        }

        protected void OnAdded(CompositeAddedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers)
            {
                e.AddedComposite.Subscribe(observer);
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

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            foreach (IComposite completeAnswer in Children)
            {
                completeAnswer.Subscribe(observer);
            }
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
