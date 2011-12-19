using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NCalc;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class Question /*: IEntity<QuestionDocument>*/ 
    {

        public Question()
        {
            PublicKey = Guid.NewGuid();
            Answers= new List<Answer>();
        }
        public Question(Questionnaire owner, string text, QuestionType type): this()
        {
            //innerDocument = new QuestionDocument() { QuestionText = text, QuestionType = type, QuestionnaireId = owner.QuestionnaireId };
            QuestionnaireId = owner.QuestionnaireId;
            QuestionText = text;
            QuestionType = type;
        }
        public Guid PublicKey { get; set; }
        public string QuestionText{ get; set; }
        public QuestionType QuestionType { get; set; }
        public string QuestionnaireId { get; set; }
        public List<Answer> Answers { get; set; }
        public string ConditionExpression { get; private set; }
        
        public void ClearAnswers()
        {
            Answers.Clear();
        }

        public void AddAnswer(Answer answer)
        {
           // answer.QuestionId = this.QuestionId;
            if(Answers.Where(a => a.PublicKey.Equals(answer.PublicKey)).Count()>0)
                throw  new DuplicateNameException("answer with current publick key already exist");
            Answers.Add(answer);
        }
        public void UpdateAnswerList(IEnumerable<Answer> answers)
        {
            ClearAnswers();
            foreach (var answer in answers)
            {
                AddAnswer(answer);
            }
        }
        public bool EvaluateCondition(IList<CompleteAnswer> answers)
        {
            if (string.IsNullOrEmpty(ConditionExpression))
                return true;
            var e = new Expression(ConditionExpression);
            foreach (var answer in answers)
            {
               /* var answerData = Answers.Where(a => a.PublicKey.Equals(answer.PublicKey)).FirstOrDefault();
                if (answerData == null)
                {
                    continue;
                }
                var answerValue = answerData.AnswerType == AnswerType.Select
                                      ? answerData.AnswerText
                                      : answer.CustomAnswer;*/
                //Answers.Where(a=>a.PublicKey.Equals(answer.PublicKey)).FirstOrDefault().
                e.Parameters[answer.QuestionPublicKey.ToString()] = answer.CustomAnswer;
            }

            bool result = false;
            try
            {
                result = (bool) e.Evaluate();
            }
            catch (Exception)
            {
            }
            return result;
        }

        public void SetConditionExpression(string expression)
        {
            ConditionExpression = expression;
        }
    }
}
