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

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors.ExpressionExtentions;

    using NCalc;

    /// <summary>
    /// The complete questionnaire condition executor.
    /// </summary>
    public class CompleteQuestionnaireConditionExecutor
    {
        #region Fields

        /// <summary>
        /// The stack depth limit.
        /// </summary>
        private const int StackDepthLimit = 0x64; //// temporary stackoverflow insurance

        /// <summary>
        /// The hash.
        /// </summary>
        private readonly ICompleteQuestionnaireDocument doc;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireConditionExecutor"/> class.
        /// </summary>
        /// <param name="doc">
        /// The hash.
        /// </param>
        public CompleteQuestionnaireConditionExecutor(ICompleteQuestionnaireDocument doc)
        {
            this.doc = doc;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute and change state.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        public void ExecuteAndChangeStateRecursive(ICompleteGroup group)
        {

            bool? value = this.Execute(group);
            bool result = value ?? true; //// treat null as success 

            group.Enabled = result;

            foreach (IComposite child in group.Children)
            {
                var question = child as ICompleteQuestion;
                if (question != null)
                {
                    question.Enabled = result && (this.ExecuteAndChangeInternal(question, 1) ?? true); ////method could not be executed if result is false
                    continue;
                }

                var gr = child as ICompleteGroup;
                if (gr != null && !gr.IsGroupPropagationTemplate())
                {
                    this.ExecuteAndChangeStateRecursive(gr);
                    ////gr.Enabled = result && (this.Execute(gr) ?? true); ////method could not be executed if result is false
                }
            }
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
        public bool? Execute(IConditional question)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
            {
                return true;
            }

            const int StackDepth = 1;
            return this.ExecuteAndChangeInternal(question, StackDepth);
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
        public bool? Execute(ICompleteGroup group)
        {
            return this.Execute(group as IConditional);
        }

        /// <summary>
        /// The execute intternal.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The <see cref="bool?"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        private bool? ExecuteAndChangeInternal(IConditional question, int currentStack)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
            {
                return true;
            }

            if (currentStack++ >= StackDepthLimit)
            {
                throw new Exception("Unsupported depth of expression call.");
            }

            var expression = new Expression(question.ConditionExpression);
            expression.EvaluateParameter += (name, args) =>
            {
                Guid nameGuid = Guid.Parse(name);

                var item = question as ICompleteItem;
                Guid? propagationKey = item != null ? item.PropagationPublicKey : null;
                var targetQuestion = this.doc.GetQuestion(nameGuid, propagationKey);

                if (targetQuestion != null && !string.IsNullOrWhiteSpace(targetQuestion.ConditionExpression))
                {
                    bool? value = this.ExecuteAndChangeInternal(targetQuestion, currentStack);
                    targetQuestion.Enabled = value ?? true;
                }

                if (targetQuestion == null || !targetQuestion.Enabled)
                {
                    args.Result = null;
                    return;
                }

                args.Result = targetQuestion.GetAnswerObject();
            };

            expression.EvaluateFunction += ExtentionFunctions.EvaluateFunctionContains; ////support for multioption

            //// if condition is failed to execute question or group have to be active to avoid impossible to complete syrvey 
            //// we could treat null as success
            bool? result = null; 
            try
            {
                result = (bool)expression.Evaluate();
            }
            catch (Exception ex)
            {
            }

            return result;
        }
        #endregion
    }
}