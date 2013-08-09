namespace Main.Core.ExpressionExecutors
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors.ExpressionExtentions;

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
        private readonly ICompleteQuestionnaireDocument doc;

        /// <summary>
        /// Question filter
        /// </summary>
        private readonly QuestionScope scope;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireValidationExecutor"/> class.
        /// </summary>
        /// <param name="doc">
        /// The hash.
        /// </param>
        /// <param name="scope">
        /// The scope.
        /// </param>
        public CompleteQuestionnaireValidationExecutor(ICompleteQuestionnaireDocument doc, QuestionScope scope)
        {
            this.doc = doc;
            this.scope = scope;
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
            foreach (ICompleteQuestion completeQuestion in
                    group.Children.Where(c => c is ICompleteQuestion)
                    .Select(q => q as ICompleteQuestion)
                    .Where(q => q.QuestionScope <= this.scope))
            {
                completeQuestion.Valid = this.Execute(completeQuestion);
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
            foreach (ICompleteQuestion completeQuestion in this.doc.GetQuestions().Where(q => q.QuestionScope <= this.scope))
            {
                completeQuestion.Valid = this.Execute(completeQuestion);
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
            if (question.QuestionScope > this.scope)
            {
                return true;
            }

            if (!question.Enabled)
            {
                return true;
            }

            if (!question.IsAnswered())
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
                    Guid nameGuid = string.Compare("this", name, StringComparison.OrdinalIgnoreCase) == 0 ? question.PublicKey : Guid.Parse(name);

                    Guid? propagationKey = question.PropagationPublicKey;
                    var targetQuestion = this.doc.GetQuestion(nameGuid, propagationKey);
                    if (targetQuestion == null || !targetQuestion.Enabled)
                    {
                        args.Result = null;
                        return;
                    }

                    args.Result = targetQuestion.GetAnswerObject();
                };

            e.EvaluateFunction += ExtensionFunctions.EvaluateFunctionContains; ////support for multioption

            bool result = false;
            try
            {
                result = (bool)e.Evaluate();
            }
            catch
            {
                #warning no exceptions should be ignored without at least writing to log
            }

            return result;
        }

        #endregion

    }
}