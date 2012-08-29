// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireExpressionValidator.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire expression validator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    using System;
    using System.Linq;

    using NCalc;
    using NCalc.Domain;

    using RavenQuestionnaire.Core.Entities;

    /// <summary>
    /// The questionnaire expression validator.
    /// </summary>
    public class QuestionnaireExpressionValidator : IExpressionExecutor<Questionnaire, bool>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public bool Execute(Questionnaire entity, string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return true;
            }

            var e = new Expression(expression);

            e.EvaluateParameter += (name, args) =>
                {
                    if (!entity.GetAllQuestions().Any(q => q.PublicKey.ToString().Equals(name)))
                    {
                        throw new ArgumentOutOfRangeException(string.Format("Parameter {0} is invalid", name));
                    }

                    args.Result = "0";
                };
            e.Evaluate(new EvaluationTesterVisitor(e.Options));
            return true;
        }

        #endregion
    }
}