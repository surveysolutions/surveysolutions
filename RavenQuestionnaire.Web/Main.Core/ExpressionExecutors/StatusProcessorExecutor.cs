// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusProcessorExecutor.cs" company="">
//   
// </copyright>
// <summary>
//   The status processor executor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities.Complete;

    using NCalc;

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