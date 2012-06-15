using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public abstract class AbstractQuestion: IQuestion
    {
        protected AbstractQuestion()
        {
            PublicKey = Guid.NewGuid();
            Cards = new List<Image>();
            this.Triggers = new List<Guid>();
            this.observers = new List<IObserver<CompositeEventArgs>>();

        }

        protected AbstractQuestion(string text)
            : this()
        {
            QuestionText = text;
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
        public List<Image> Cards { get; set; }

        public Order AnswerOrder { get; set; }

        public bool Featured { get; set; }


        public string ConditionExpression
        {
            get { return this.conditionExpression; }
            set
            {
                this.conditionExpression = value;
                this.Triggers = parser.Execute(value);
            }
        }

        public string ValidationExpression { get; set; }

        private string conditionExpression;
        private QuestionnaireParametersParser parser = new QuestionnaireParametersParser();

        public string StataExportCaption { get; set; }

        public string Instructions { get; set; }

        public void AddCard(Image card)
        {
            if (Cards == null)
                Cards = new List<Image>();
            Cards.Add(card);
        }
        public Image RemoveCard(Guid imageKey)
        {
            if (Cards == null)
                Cards = new List<Image>();
            var card = Cards.Single(c => c.PublicKey == imageKey);
            Cards.Remove(card);
            return card;
        }
        public void UpdateCard(Guid imageKey, string title, string desc)
        {
            if (Cards == null)
                Cards = new List<Image>();
            var card = Cards.Single(c => c.PublicKey == imageKey);

            card.Title = title;
            card.Description = desc;
        }
        public abstract void Add(IComposite c, Guid? parent);

        public abstract void Remove(IComposite c);

        public abstract void Remove(Guid publicKey);

        public abstract T Find<T>(Guid publicKey) where T : class, IComposite;

        public abstract IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        public abstract T FirstOrDefault<T>(Func<T, bool> condition) where T : class;

        public abstract List<IComposite> Children { get; set; }

        public List<IObserver<CompositeEventArgs>> Observers
        {
            get { return observers; }
        }

        [JsonIgnore]
        public IComposite Parent
        {
            get { throw new NotImplementedException(); }
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            if (observers.Contains(observer))
                return null;
            return new Unsubscriber(this, observer);
        }
        private List<IObserver<CompositeEventArgs>> observers;

        #endregion

        #region Implementation of ITriggerable

        public List<Guid> Triggers { get; set; }

        #endregion

    }
}
