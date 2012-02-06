using System;
using System.Collections.Generic;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    class StatusProcessorExecutor : IExpressionExecutor<IEnumerable<ICompleteAnswer>, bool>
    {
        public bool Execute(IEnumerable<ICompleteAnswer> entity, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var expressionItem = new Expression(condition);

            foreach (var answer in entity)
            {
                expressionItem.Parameters[answer.QuestionPublicKey.ToString()] =
                    answer.AnswerValue ?? answer.AnswerText;
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
