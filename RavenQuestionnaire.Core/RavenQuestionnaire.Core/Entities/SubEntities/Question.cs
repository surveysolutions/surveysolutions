using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class Question : /*IEntity<QuestionDocument>*/ IComposite
    {

        public Question()
        {
            PublicKey = Guid.NewGuid();
            Answers = new List<Answer>();
        }
        public Question(string text, QuestionType type)
            : this()
        {
            QuestionText = text;
            QuestionType = type;
        }
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<Answer> Answers { get; set; }
        public string ConditionExpression { get; private set; }

        //remove when exportSchema will be done 
        public string StataExportCaption { get; set; }

        public void ClearAnswers()
        {
            Answers.Clear();
        }

        public void UpdateAnswerList(IEnumerable<Answer> answers)
        {
            ClearAnswers();
            foreach (Answer answer in answers)
            {
                Add(answer, PublicKey);
            }
        }
        public void SetConditionExpression(string expression)
        {
            ConditionExpression = expression;
        }

        protected void AddAnswer(Answer answer)
        {
            if (Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
                throw new DuplicateNameException("answer with current publick key already exist");
            Answers.Add(answer);
        }

      /*  public bool Remove(IComposite c, Guid? parent)
        {
            Answer answer = c as Answer;
            if (answer == null)
                throw new ArgumentException("Only answer can be removed from question");
            Answers.Remove(answer);
            return true;
        }
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return Answers.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
        }*/

        public bool Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                Answer answer = c as Answer;
                if (answer != null)
                {
                    AddAnswer(answer);
                    return true;
                }
            }
            return false;
        }

        public bool Remove(IComposite c)
        {
            Answer answer = c as Answer;
            if (answer != null)
            {
                Answers.Remove(answer);
                return true;
            }
            return false;
        }
        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(Answer))
            {
                return Answers.RemoveAll(a => a.PublicKey.Equals(publicKey)) > 0;
            }
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof (T) == typeof (Answer))
                return Answers.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
            return null;
        }
    }
}
