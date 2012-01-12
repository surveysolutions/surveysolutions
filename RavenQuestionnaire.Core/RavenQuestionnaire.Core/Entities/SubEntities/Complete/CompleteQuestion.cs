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
        public void AddAnswer(CompleteAnswer answer)
        {
            CompleteAnswer completeAnswer =
                this.Answers.FirstOrDefault(a => a.PublicKey.Equals(answer.PublicKey));
            if (completeAnswer == null)
                throw new ArgumentException(string.Format("answer with guid {0} doesn't exists in current question",
                                                          answer.PublicKey));
            completeAnswer.Set(answer.CustomAnswer);
        }
        public  bool Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                CompleteAnswer answer = c as CompleteAnswer;
                if (answer != null)
                {
                    this.AddAnswer(answer);
                    return true;
                }
            }
            return false;
        }

        public  bool Remove(IComposite c)
        {
            CompleteAnswer answer = c as CompleteAnswer;
            if (answer != null)
            {
                answer.Reset();
                return true;
            }
            return false;
        }
        public  bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(CompleteAnswer))
            {
                foreach (CompleteAnswer completeAnswer in Answers)
                {
                    completeAnswer.Reset();
                }
                return true;
            }
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(CompleteAnswer))
                return Answers.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
            return null;
        }
    }
}
