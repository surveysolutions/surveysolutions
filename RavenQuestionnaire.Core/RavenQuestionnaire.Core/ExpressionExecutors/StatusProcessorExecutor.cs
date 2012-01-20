using System;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    class StatusProcessorExecutor : IExpressionExecutor<CompleteQuestionnaire, bool>
    {
        public bool Execute(CompleteQuestionnaire entity, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var expressionItem = new Expression(condition);

            foreach (var answer in entity.AnswerIterator)
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
