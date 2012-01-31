using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteQuestion : ICompleteQuestion<CompleteAnswer>
    {
        public CompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            Answers = new List<CompleteAnswer>();
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
                StataExportCaption = doc.StataExportCaption
            };
            result.Answers = doc.Answers.Select(a => (CompleteAnswer)a).ToList();
            return result;
        }
        public Guid PublicKey { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public List<CompleteAnswer> Answers { get; set; }

        public string ConditionExpression { get; set; }

        public string StataExportCaption { get; set; }

        public bool Enabled { get; set; }
        public void Add(IComposite c, Guid? parent)
        {
            new CompleteQuestionFactory().Create(this).Add(c, parent);
        }

        public void Remove(IComposite c)
        {
            CompleteQuestion question = c as CompleteQuestion;
            if (question != null && this.PublicKey.Equals(question.PublicKey))
            {
                foreach (CompleteAnswer answer in this.Answers)
                {
                    answer.Remove(answer);
                }
                return;
            }
            if (c as CompleteAnswer == null)
                foreach (CompleteAnswer completeAnswer in this.Answers)
                {
                    try
                    {
                        completeAnswer.Remove(c);
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
                return;
            }
            if (typeof (T) != typeof (CompleteAnswer))
                foreach (CompleteAnswer completeAnswer in this.Answers)
                {
                    try
                    {
                        completeAnswer.Remove<T>(publicKey);
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
            if (typeof(T) == GetType())
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            if (typeof(T) == typeof(CompleteAnswer))
            {
                return this.Answers.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            }
            return null;
        }
    }
}
