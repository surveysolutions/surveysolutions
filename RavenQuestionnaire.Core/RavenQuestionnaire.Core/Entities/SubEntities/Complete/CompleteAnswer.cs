using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteAnswer : IAnswer
    {
       bool Selected { get; set; }
       Guid? PropogationPublicKey { get; set; }
    }

    public class CompleteAnswer : ICompleteAnswer
    {
        public CompleteAnswer()
        {
            this.PublicKey = Guid.NewGuid();
            this.observers=new List<IObserver<CompositeEventArgs>>();
        }

        public CompleteAnswer(IAnswer answer):this()
        {
            this.AnswerText = answer.AnswerText;
            this.AnswerType = answer.AnswerType;
            this.AnswerValue = answer.AnswerValue;
            this.AnswerImage = answer.AnswerImage;
            this.Image = answer.Image;
            this.Mandatory = answer.Mandatory;
            this.PublicKey = answer.PublicKey;
            this.Selected = false;
       
          /*  this.PublicKey = answer.PublicKey;
            this.QuestionPublicKey = questionPublicKey;*/
            // this.CustomAnswer = answer.AnswerText;
        }
        public CompleteAnswer(ICompleteAnswer answer, Guid? propogationPublicKey)
            : this(answer)
        {
            this.Selected = answer.Selected;
            this.PropogationPublicKey = propogationPublicKey;
        }
        public static explicit operator CompleteAnswer(Answer doc)
        {
            return new CompleteAnswer
            {
                PublicKey = doc.PublicKey,
                AnswerText = doc.AnswerText,
                AnswerValue = doc.AnswerValue,
                Mandatory = doc.Mandatory,
                AnswerType = doc.AnswerType,
                AnswerImage = doc.AnswerImage,
                Image =  doc.Image
            };
        }
        public Guid PublicKey { get; set; }
        public string AnswerText { get; set; }

        public string AnswerImage { get; set; }

        public bool Mandatory { get; set; }
        public AnswerType AnswerType { get; set; }
        public string NameCollection { get; set; }
        public Image Image { get; set; }
        
        public object AnswerValue { get; set; }
        public bool Selected { get; set; }

        public Guid? PropogationPublicKey { get; set; }

        protected void Set(object text)
        {
            this.Selected = true;
          //  if (this.AnswerType == AnswerType.Text)
           //     this.AnswerValue = text;
        }

        protected void Reset()
        {
            this.Selected = false;
          //  if (this.AnswerType == AnswerType.Text)
          //  this.AnswerValue = null;
        }

        #region Implementation of IComposite

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer answer = c as CompleteAnswer;
            if (answer == null)
                throw new CompositeException("answer wasn't found");
            if (answer.PublicKey == PublicKey &&
                ((!answer.PropogationPublicKey.HasValue && !this.PropogationPublicKey.HasValue) ||
                 answer.PropogationPublicKey == this.PropogationPublicKey))
            {
                Set(answer.AnswerValue);
                return;
            }
            throw new CompositeException("answer wasn't found");
        }

        public void Remove(IComposite c)
        {
            if (c.PublicKey == PublicKey)
            {
                Reset();
                return;
            }
            throw new CompositeException("answer wasn't found");
        }
        public void Remove(Guid publicKey)
        {
            if (publicKey == PublicKey)
            {
                Reset();
                return;
            }
            throw new CompositeException("answer wasn't found");
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (!typeof(T).IsAssignableFrom(GetType()))
                return null;
            if (publicKey == PublicKey)
            {
                return this as T;
            }
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {

            if (!typeof(T).IsAssignableFrom(GetType()))
                return new T[0];
            if (condition(this as T))
            {
                return new T[] {this as T};
            }
            return new T[0];
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            if (!typeof(T).IsAssignableFrom(GetType()))
                return null;
            if (condition(this as T))
            {
                return  this as T ;
            }
            return null;
        }

        public List<IComposite> Children
        {
            get { return new List<IComposite>(); }
            set { }
        }

        public List<IObserver<CompositeEventArgs>> Observers
        {
            get { return observers; }
        }

        [JsonIgnore]
        public IComposite Parent
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
        protected void OnAdded(CompositeAddedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers.ToList())
            {
                observer.OnNext(e);
            }
        }
        protected void OnRemoved(CompositeRemovedEventArgs e)
        {
            foreach (IObserver<CompositeEventArgs> observer in observers.ToList())
            {
                observer.OnNext(e);
            }
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
    }
}
