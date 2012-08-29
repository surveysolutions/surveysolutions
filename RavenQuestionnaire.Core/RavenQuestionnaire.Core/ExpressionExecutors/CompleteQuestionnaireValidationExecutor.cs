// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireValidationExecutor.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire validation executor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    using System;
    using System.Linq;

    using NCalc;

    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete questionnaire validation executor.
    /// </summary>
    public class CompleteQuestionnaireValidationExecutor
    {
        #region Fields

        /// <summary>
        /// The hash.
        /// </summary>
        private readonly GroupHash hash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireValidationExecutor"/> class.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        public CompleteQuestionnaireValidationExecutor(GroupHash hash)
        {
            this.hash = hash;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        public void Execute(ICompleteGroup group)
        {
            foreach (ICompleteQuestion completeQuestion in group.Children.Where(c => c is ICompleteQuestion))
            {
                completeQuestion.Valid = Execute(completeQuestion);
            }
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Execute()
        {
            bool isValid = true;
            foreach (ICompleteQuestion completeQuestion in this.hash.Questions)
            {
                completeQuestion.Valid = Execute(completeQuestion);
                isValid = isValid && completeQuestion.Valid;
            }

            return isValid;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        protected bool Execute(ICompleteQuestion question)
        {
            if (!question.Enabled)
            {
                return true;
            }

            if (string.IsNullOrEmpty(question.ValidationExpression))
            {
                return true;
            }

            string expression = question.ValidationExpression.ToLower();
            if (expression.Contains("this"))
            {
                expression = expression.Replace("this", question.PublicKey.ToString());
            }

            var e = new Expression(expression);
            e.EvaluateParameter += (name, args) =>
                {
                    Guid nameGuid = Guid.Parse(name);
                    Guid? propagationKey = question.PropogationPublicKey;
                    object value = this.hash[nameGuid, propagationKey].GetAnswerObject();
                    args.Result = value ?? string.Empty;
                };
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

        #endregion
    }
}