using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IQuestion : IComposite
    {
        Guid PublicKey { get; set; }
        string QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        string ConditionExpression { get; set; }
        string StataExportCaption { get; set; }
    }

    public interface IQuestion<T> : IQuestion where T : IAnswer
    {
        List<T> Answers { get; set; }
    }

    public class Question : /*IEntity<QuestionDocument>*/IQuestion<Answer>
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
        public string ConditionExpression { get; set; }

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
        public void AddAnswer(Answer answer)
        {
            if (Answers.Any(a => a.PublicKey.Equals(answer.PublicKey)))
                throw new DuplicateNameException("answer with current publick key already exist");
            Answers.Add(answer);
        }

        public void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue || parent.Value == PublicKey)
            {
                Answer answer = c as Answer;
                if (answer != null)
                {
                    AddAnswer(answer);
                    return;
                }
            }
            throw new CompositeException();
        }

        public void Remove(IComposite c)
        {
            Answer answer = c as Answer;
            if (answer != null)
            {
                Answers.Remove(answer);
                return;
            }
            throw new CompositeException();
        }
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof(T) == typeof(Answer))
            {
                if(Answers.RemoveAll(a => a.PublicKey.Equals(publicKey)) > 0)
                    return;
            }
            throw new CompositeException();
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            if (typeof (T) == typeof (Answer))
                return Answers.FirstOrDefault(a => a.PublicKey.Equals(publicKey)) as T;
            return null;
        }
    }
}
