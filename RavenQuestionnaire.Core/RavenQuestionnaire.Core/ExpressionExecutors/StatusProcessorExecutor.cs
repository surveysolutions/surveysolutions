using System;
using System.Collections.Generic;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    class StatusProcessorExecutor : IExpressionExecutor<IEnumerable<CompleteAnswer>, bool>
    {
        public bool Execute(IEnumerable<CompleteAnswer> entity, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var expressionItem = new Expression(condition);

            foreach (var answer in entity)
            {
                expressionItem.Parameters[answer.QuestionPublicKey.ToString()] = 
                    answer.AnswerType == AnswerType.Text
                    ? answer.CustomAnswer
                    : answer.AnswerText;
            }

            bool result = false;
            try
            {
                result = (bool)expressionItem.Evaluate();
            }
            catch (Exception)
            {
            }

            return result;
        }
    }
}
