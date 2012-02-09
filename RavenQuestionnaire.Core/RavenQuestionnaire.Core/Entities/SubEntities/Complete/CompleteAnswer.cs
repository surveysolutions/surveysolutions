using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteAnswer : IAnswer
    {
       bool Selected { get; set; }
       Guid QuestionPublicKey { get; set; }
    }

    public class CompleteAnswer : ICompleteAnswer
    {
        public CompleteAnswer()
        {
            this.PublicKey = Guid.NewGuid();
        }

        public CompleteAnswer(IAnswer answer, Guid questionPublicKey)
        {
            this.AnswerText = answer.AnswerText;
            this.AnswerType = answer.AnswerType;
            this.AnswerValue = answer.AnswerValue;
            this.Mandatory = answer.Mandatory;
            this.PublicKey = answer.PublicKey;
            this.Selected = false;
            this.QuestionPublicKey = questionPublicKey;
       
          /*  this.PublicKey = answer.PublicKey;
            this.QuestionPublicKey = questionPublicKey;*/
            // this.CustomAnswer = answer.AnswerText;
        }
        public static explicit operator CompleteAnswer(Answer doc)
        {
            return new CompleteAnswer
            {
                PublicKey = doc.PublicKey,
                AnswerText = doc.AnswerText,
                AnswerValue = doc.AnswerValue,
                Mandatory = doc.Mandatory,
                AnswerType = doc.AnswerType
            };
        }
        public Guid PublicKey { get; set; }
        public string AnswerText { get; set; }
        public bool Mandatory { get; set; }
        public AnswerType AnswerType { get; set; }
        public object AnswerValue { get; set; }
        [XmlIgnore]
        public Guid QuestionPublicKey { get; set; }
        public bool Selected { get; set; }
        protected void Set(object text)
        {
            this.Selected = true;
            if (this.AnswerType == AnswerType.Text)
                this.AnswerValue = text;
        }

        protected void Reset()
        {
            this.Selected = false;
            if (this.AnswerType == AnswerType.Text)
                this.AnswerValue = null;
        }

        #region Implementation of IComposite

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer answer = c as CompleteAnswer;
            if (answer == null)
                throw new CompositeException("answer wasn't found");
            if (answer.PublicKey == PublicKey)
            {
                Set(answer.AnswerValue);
                return;
            }
            throw new CompositeException("answer wasn't found");
        }

        public void Remove(IComposite c)
        {
            CompleteAnswer answer = c as CompleteAnswer;
            if (answer == null)
                throw new CompositeException("answer wasn't found");
            if (answer.PublicKey == PublicKey)
            {
                Reset();
                return;
            }
            throw new CompositeException("answer wasn't found");
        }
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) != GetType())
                throw new CompositeException("answer wasn't found");
            if (publicKey == PublicKey)
            {
                Reset();
                return;
            }
            throw new CompositeException("answer wasn't found");
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) != GetType())
                return null;
            if (publicKey == PublicKey)
            {
                return this as T;
            }
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class, IComposite
        {
            
            if (typeof (T) != GetType())
                return new T[0];
            if (condition(this as T))
            {
                return new T[] {this as T};
            }
            return new T[0];
        }

        #endregion
    }
}
