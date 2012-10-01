// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireConditionExecutor.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire condition executor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.ExpressionExecutors
{
    using System;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities.Complete;

    using NCalc;

    /// <summary>
    /// The complete questionnaire condition executor.
    /// </summary>
    public class CompleteQuestionnaireConditionExecutor
    {
        #region Fields

        /// <summary>
        /// The hash.
        /// </summary>
        private readonly GroupHash hash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireConditionExecutor"/> class.
        /// </summary>
        /// <param name="hash">
        /// The hash.
        /// </param>
        public CompleteQuestionnaireConditionExecutor(GroupHash hash)
        {
            this.hash = hash;
        }

        #endregion

        #region Public Methods and Operators



        /*/// <summary>
        /// The execute.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Execute(<T> executable)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
            {
                return true;
            }

            var expression = new Expression(question.ConditionExpression);
            expression.EvaluateParameter += (name, args) =>
            {
                Guid nameGuid = Guid.Parse(name);
                Guid? propagationKey = question.PropogationPublicKey;
                var targetQuestion = this.hash[nameGuid, propagationKey];
                if (!targetQuestion.Enabled)
                {
                    args.Result = null;
                    return;
                }
                args.Result = targetQuestion.GetAnswerObject();
            };
            bool result = false;
            try
            {
                result = (bool)expression.Evaluate();
            }
            catch (Exception)
            {
            }

            return result;
        }
*/

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
            if (string.IsNullOrEmpty(question.ConditionExpression))
            {
                return true;
            }

            var expression = new Expression(question.ConditionExpression);
            expression.EvaluateParameter += (name, args) =>
                {
                    Guid nameGuid = Guid.Parse(name);
                    Guid? propagationKey = question.PropogationPublicKey;
                    var targetQuestion = this.hash[nameGuid, propagationKey];
                    if(!targetQuestion.Enabled)
                    {
                        args.Result = null;
                        return;
                    }
                    args.Result = targetQuestion.GetAnswerObject();
                };
            bool result = false;
            try
            {
                result = (bool)expression.Evaluate();
            }
            catch (Exception)
            {
            }

            return result;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Execute(ICompleteGroup group)
        {
            bool result = this.ExecuteGroup(group);
            this.UpdateAllChildElementsInGroup(group, result);
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute group.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        private bool ExecuteGroup(ICompleteGroup group)
        {
            if (string.IsNullOrEmpty(group.ConditionExpression))
            {
                return true;
            }

            var expression = new Expression(group.ConditionExpression);
            expression.EvaluateParameter += (name, args) =>
                {
                    Guid nameGuid = Guid.Parse(name);
                    Guid? propagationKey = group.PropogationPublicKey;
                    object value = this.hash[nameGuid, propagationKey].GetAnswerObject();
                    args.Result = value ?? string.Empty;
                };
            bool result = false;
            try
            {
                result = (bool)expression.Evaluate();
            }
            catch (Exception)
            {
            }

            return result;
        }

        /// <summary>
        /// The update all child elements in group.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        private void UpdateAllChildElementsInGroup(ICompleteGroup group, bool result)
        {
            foreach (IComposite child in group.Children)
            {

                var question = child as ICompleteQuestion;
                if (question != null)
                {
                    
                    question.Enabled = result && this.Execute(question); ////method could not be executed if result is false
                    continue;
                }

                var gr = child as ICompleteGroup;
                if (gr != null && !gr.IsGroupPropagationTemplate())
                {
                    gr.Enabled = result && this.Execute(gr); ////method could not be executed if result is false
                }
            }
        }

        #endregion
    }
}