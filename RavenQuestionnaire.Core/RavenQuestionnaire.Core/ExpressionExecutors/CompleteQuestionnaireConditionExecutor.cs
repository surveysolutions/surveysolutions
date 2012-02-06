using System;
using System.Collections.Generic;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor : IExpressionExecutor<IEnumerable<ICompleteAnswer>, bool>
    {
        public bool Execute(IEnumerable<ICompleteAnswer> answers, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var e = new Expression(condition);
            foreach (var answer in answers)
            {
                e.Parameters[answer.QuestionPublicKey.ToString()] = answer.AnswerValue ?? answer.AnswerText;
            }

            bool result = false;
            try
            {
                result = (bool)e.Evaluate();
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
