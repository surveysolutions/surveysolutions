using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.ExpressionExecutors.ExpressionExtentions;
using NCalc;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.Validation
{
    public class QuestionnaireValidationExecutor : IQuestionnaireValidationExecutor
    {
          #region Fields

        /// <summary>
        /// The hash.
        /// </summary>
        private readonly CompleteQuestionnaireView doc;

        private readonly IEnumerable<QuestionViewModel> questionsForValidation; 
        #endregion

        #region Constructors and Destructors

       
        public QuestionnaireValidationExecutor(CompleteQuestionnaireView doc)
        {
            this.doc = doc;
            this.questionsForValidation =
                this.doc.FindQuestion(q => !string.IsNullOrEmpty(q.ValidationExpression) || q.Mandatory);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool Execute()
        {
            bool isValid = true;
            foreach (QuestionViewModel completeQuestion in questionsForValidation)
            {
                completeQuestion.SetValid(this.Execute(completeQuestion));
                isValid = isValid && completeQuestion.Status.HasFlag(QuestionStatus.Valid);
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
        public bool Execute(QuestionViewModel question)
        {

            if (!question.Status.HasFlag(QuestionStatus.Enabled))
            {
                return true;
            }

            if (!question.Status.HasFlag(QuestionStatus.Answered))
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
                    name = name.Trim();
                    var targetQuestion = string.Compare("this", name, StringComparison.OrdinalIgnoreCase) == 0
                                       ? question
                                       : doc.FindQuestion(q => q.PublicKey == new ItemPublicKey(Guid.Parse(name), question.PublicKey.PropagationKey)).FirstOrDefault();

                    if (targetQuestion == null || !targetQuestion.Status.HasFlag(QuestionStatus.Enabled))
                    {
                        args.Result = null;
                        return;
                    }

                    args.Result = TryToMakeType(targetQuestion);
                };

            e.EvaluateFunction += ExtensionFunctions.EvaluateFunctionContains; ////support for multioption

            bool result = false;
            try
            {
                result = (bool)e.Evaluate();
            }
            catch (Exception exc)
            {
            }

            return result;
        }

        #endregion
        /// <summary>
        /// very very bad method, in order to make a hot fix of numeric value
        /// TODO clean up this shit
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        protected object TryToMakeType(QuestionViewModel question)
        {
            if (question.QuestionType != QuestionType.Numeric)
            {
                return question.AnswerObject;
            }
            int val;
            if (int.TryParse(question.AnswerObject, out val))
                return val;
            return question.AnswerObject;
        }
    }
}