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

        public CompleteAnswer(IAnswer answer , Guid questionPublicKey)
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
                Mandatory = doc.Mandatory,
                AnswerType = doc.AnswerType
            };
        }
        public Guid PublicKey { get; set; }
        public string AnswerText { get; set; }
        public bool Mandatory { get; set; }
        public AnswerType AnswerType { get; set; }
        public string CustomAnswer { get; set; }
        [XmlIgnore]
        public Guid QuestionPublicKey { get; set; }
        public bool Selected { get; set; }
        protected void Set(string text)
        {
            this.Selected = true;
            this.CustomAnswer = text;
        }
        protected void Reset()
        {
            this.Selected = false;
            this.CustomAnswer = null;
        }

        #region Implementation of IComposite

        public bool Add(IComposite c, Guid? parent)
        {
            CompleteAnswer answer = c as CompleteAnswer;
            if (answer == null)
                return false;
            if (answer.PublicKey == PublicKey)
            {
                Set(answer.CustomAnswer);
                return true;
            }
            return false;
        }

        public bool Remove(IComposite c)
        {
            CompleteAnswer answer = c as CompleteAnswer;
            if (answer == null)
                return false;
            if (answer.PublicKey == PublicKey)
            {
                Reset();
                return true;
            }
            return false;
        }
        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) != GetType())
                return false;
            if (publicKey == PublicKey)
            {
                Reset();
                return true;
            }
            return false;
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
