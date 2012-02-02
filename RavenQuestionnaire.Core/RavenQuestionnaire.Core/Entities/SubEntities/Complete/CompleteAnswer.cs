using System;
using System.Xml.Serialization;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteAnswer : IAnswer
    {
        public CompleteAnswer()
        {
            this.PublicKey = Guid.NewGuid();
        }

        public CompleteAnswer(IAnswer answer, Guid questionPublicKey)
        {
            this.PublicKey = answer.PublicKey;
            this.QuestionPublicKey = questionPublicKey;
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

        #endregion
    }
}
