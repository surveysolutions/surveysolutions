using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class CompleteQuestion : IQuestion<CompleteAnswer>
    {
        public CompleteQuestion()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            Answers = new List<CompleteAnswer>();
        }

        public CompleteQuestion(string text, QuestionType type):this()
        {

            this.QuestionText = text;
            this.QuestionType = type;
        }
        public static explicit operator CompleteQuestion (Question doc)
        {
            CompleteQuestion result= new CompleteQuestion
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

        public  List<CompleteAnswer> Answers { get; set; }

        public string ConditionExpression { get; private set; }

        public string StataExportCaption { get; set; }

        public bool Enabled { get; set; }

    /*    public void ClearAnswers()
        {
            foreach (CompleteAnswer answer in this.Answers)
            {
                answer.Selected = false;
                answer.CustomAnswer = null;
            }
        }

        public void UpdateAnswerList(IEnumerable<CompleteAnswer> answers)
        {
            ClearAnswers();
            foreach (CompleteAnswer answer in answers)
            {
                Add(answer, PublicKey);
            }
        }*/

        public void SetConditionExpression(string expression)
        {
            ConditionExpression = expression;
        }
      /*  public void AddAnswer(CompleteAnswer answer)
        {
            CompleteAnswer completeAnswer =
                this.Answers.FirstOrDefault(a => a.PublicKey.Equals(answer.PublicKey));
            if (completeAnswer == null)
                throw new ArgumentException(string.Format("answer with guid {0} doesn't exists in current question",
                                                          answer.PublicKey));
            completeAnswer.Set(answer.CustomAnswer);
        }*/
        public  bool Add(IComposite c, Guid? parent)
        {
            if (c as CompleteAnswer == null)
                return false;
            return Answers.Any(answer => answer.Add(c, parent));
        }

        public bool Remove(IComposite c)
        {
            CompleteQuestion question = c as CompleteQuestion;
            if (question != null && this.PublicKey.Equals(question.PublicKey))
            {
                foreach (CompleteAnswer answer in Answers)
                {
                    answer.Remove(answer);
                }
                return true;
            }
            if (c as CompleteAnswer == null)
                return Answers.Any(answer => answer.Remove(c));
            return false;
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == GetType() && this.PublicKey.Equals(publicKey))
            {
                foreach (CompleteAnswer answer in Answers)
                {
                    answer.Remove(answer);
                }
                return true;
            }
            if (typeof (T) != typeof (CompleteAnswer))
                return Answers.Any(answer => answer.Remove<T>(publicKey));
            return false;
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
                return Answers.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            }
            return null;
        }
    }
}
