// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusProcessorExecutor.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The status processor executor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;

    using NCalc;

    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The status processor executor.
    /// </summary>
    internal class StatusProcessorExecutor : IExpressionExecutor<ICompleteGroup, bool>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Execute(ICompleteGroup entity, string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return true;
            }

            var expressionItem = new Expression(condition);
            IEnumerable<ICompleteQuestion> questions = entity.GetAllQuestions<ICompleteQuestion>();
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                IEnumerable<ICompleteAnswer> answers = completeQuestion.Find<ICompleteAnswer>(a => a.Selected);
                foreach (ICompleteAnswer answer in answers)
                {
                    expressionItem.Parameters[completeQuestion.PublicKey.ToString()] = answer.AnswerValue
                                                                                       ?? answer.AnswerText;
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

        #endregion
    }
}