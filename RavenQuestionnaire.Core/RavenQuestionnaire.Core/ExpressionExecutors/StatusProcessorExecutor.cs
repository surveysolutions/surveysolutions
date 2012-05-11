using System;
using System.Collections.Generic;
using NCalc;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    class StatusProcessorExecutor : IExpressionExecutor<ICompleteGroup, bool>
    {
        public bool Execute(ICompleteGroup entity, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var expressionItem = new Expression(condition);
            var questions = entity.GetAllQuestions<ICompleteQuestion>();
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                var answers = completeQuestion.Find<ICompleteAnswer>(a => a.Selected);
                foreach (var answer in answers)
                {
                    expressionItem.Parameters[completeQuestion.PublicKey.ToString()] =
                        answer.AnswerValue ?? answer.AnswerText;
                }
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
