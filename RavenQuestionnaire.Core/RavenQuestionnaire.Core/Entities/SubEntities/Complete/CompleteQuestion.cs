using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteQuestion : ICompleteQuestion<ICompleteAnswer>
    {
        public CompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            this.Answers = new List<ICompleteAnswer>();
            this.Triggers = new List<Guid>();
            //this.Answers.GetObservablePropertyChanges().Subscribe(e=>e.EventArgs)
            this.observers = new List<IObserver<CompositeEventArgs>>();
            SubscribeAddedAnswers();
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
                                              Triggers = doc.Triggers
                                          };

            foreach (IAnswer answer in doc.Answers)
            {
                var newanswer = new CompleteAnswerFactory().ConvertToCompleteAnswer(answer);
                result.Answers.Add(newanswer);
                result.OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(result), newanswer));
            }
            return result;
        }

        protected void SubscribeAddedAnswers()
        {
            this.GetAllAnswerAddedEvents().Subscribe(
                    Observer.Create<CompositeAddedEventArgs>(HandleAddedAnswer));
        }
        protected void HandleAddedAnswer(CompositeAddedEventArgs e)
        {
            ((ICompleteAnswer)e.AddedComposite).QuestionPublicKey = this.PublicKey;
        }

        public Guid PublicKey { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public List<ICompleteAnswer> Answers
        {
            get { return answers; }
            set
            {
                answers = value;
                foreach (ICompleteAnswer completeAnswer in answers)
                {
                    OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), completeAnswer));
                }
            }
        }

        private List<ICompleteAnswer> answers;


        public string ConditionExpression { get; set; }

        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public List<Image> Cards { get; set; }

        public bool Enabled { get; set; }
        public void Add(IComposite c, Guid? parent)
        {
            new CompleteQuestionFactory().Create(this).Add(c, parent);
            OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(this), c));
        }

        public void Remove(IComposite c)
        {
            CompleteQuestion question = c as CompleteQuestion;
            if (question != null)
            {
                if (!this.PublicKey.Equals(question.PublicKey))
                    throw new CompositeException();
                foreach (CompleteAnswer answer in this.Answers)
                {
                    answer.Remove(answer);

                }
                OnRemoved(new CompositeRemovedEventArgs(this));
                return;
            }
            if (c as CompleteAnswer != null)
                foreach (CompleteAnswer completeAnswer in this.Answers)
                {
                    try
                    {
                        completeAnswer.Remove(c);
                        OnRemoved(new CompositeRemovedEventArgs(new CompositeRemovedEventArgs(this), completeAnswer));
                        return;
                    }
                    catch (CompositeException)
                    {
                    }
                }
            throw new CompositeException("answer wasn't found");
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == GetType() && this.PublicKey.Equals(publicKey))
            {
                foreach (CompleteAnswer answer in this.Answers)
                {
                    answer.Remove(answer);
                }
                OnRemoved(new CompositeRemovedEventArgs(this));
                return;
            }
            if (typeof(T) != typeof(CompleteAnswer))
                foreach (CompleteAnswer completeAnswer in this.Answers)
                {
                    try
                    {
                        completeAnswer.Remove<T>(publicKey);
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
                return this.Answers.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            }
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                Answers.Where(a => a is T && condition(a as T)).Select
                    (a => a as T);
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return Find<T>(condition).FirstOrDefault();
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
            foreach (ICompleteAnswer completeAnswer in Answers)
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
