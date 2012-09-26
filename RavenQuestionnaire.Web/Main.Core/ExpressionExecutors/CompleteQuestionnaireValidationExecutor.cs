// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireValidationExecutor.cs" company="">
//   
// </copyright>
// <summary>
//   The complete questionnaire validation executor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.ExpressionExecutors
{
    using System;
    using System.Linq;

    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities.Complete;

    using NCalc;

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

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Execute(ICompleteQuestion question)
        {
            if (!question.Enabled)
            {
                return true;
            }

            if (question.GetAnswerObject() == null)
            {
                return !question.Mandatory;
            }

            if (string.IsNullOrEmpty(question.ValidationExpression))
            {
                return true;
            }

            var e = new Expression(question.ValidationExpression);
            e.EvaluateParameter += (name, args) =>
                {
                    Guid nameGuid;
                    if (string.Compare("this", name, true) == 0)
                    {
                        nameGuid = question.PublicKey;
                    }
                    else
                    {
                        nameGuid = Guid.Parse(name);
                    }

                    Guid? propagationKey = question.PropogationPublicKey;
                    var targetQuestion = this.hash[nameGuid, propagationKey];
                    if (!targetQuestion.Enabled)
                    {
                        args.Result = null;
                        return;
                    }
                    args.Result = this.hash[nameGuid, propagationKey].GetAnswerObject();
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