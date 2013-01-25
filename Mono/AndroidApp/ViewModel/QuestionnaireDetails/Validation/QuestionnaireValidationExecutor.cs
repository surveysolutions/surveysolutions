using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Main.Core.ExpressionExecutors.ExpressionExtentions;
using NCalc;

namespace AndroidApp.ViewModel.QuestionnaireDetails.Validation
{
    public class QuestionnaireValidationExecutor
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
                    var nameGuid = string.Compare("this", name, StringComparison.OrdinalIgnoreCase) == 0
                                       ? question.PublicKey
                                       : new ItemPublicKey(Guid.Parse(name), question.PublicKey.PropagationKey);

               //     Guid? propagationKey = question.PropagationPublicKey;
                    var targetQuestion = /* this.doc.GetQuestion(nameGuid, propagationKey);*/
                        doc.FindQuestion(q => q.PublicKey == nameGuid).FirstOrDefault();
                    if (targetQuestion == null || !targetQuestion.Status.HasFlag(QuestionStatus.Enabled))
                    {
                        args.Result = null;
                        return;
                    }

                    args.Result = targetQuestion.AnswerObject;
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
    }
}