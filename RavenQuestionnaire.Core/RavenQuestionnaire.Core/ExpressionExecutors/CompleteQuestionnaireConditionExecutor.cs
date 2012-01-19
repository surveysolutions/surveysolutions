using System;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor : IExpressionExecutor<CompleteQuestionnaire, bool>
    {
        public bool Execute(CompleteQuestionnaire entity, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var e = new Expression(condition);
            foreach (var answer in entity.GetAllAnswers())
            {
                e.Parameters[answer.QuestionPublicKey.ToString()] = answer.AnswerType == AnswerType.Text
                                                                        ? answer.CustomAnswer
                                                                        : answer.AnswerText;
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
    }
}
