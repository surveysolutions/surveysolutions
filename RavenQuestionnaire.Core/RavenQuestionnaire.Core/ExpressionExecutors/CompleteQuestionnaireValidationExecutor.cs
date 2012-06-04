using System;
using NCalc;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireValidationExecutor
    {
        private readonly ICompleteGroup questionnaire;
        public CompleteQuestionnaireValidationExecutor(ICompleteGroup questionnaire)
        {
            this.questionnaire = questionnaire;
        }
        public bool Execute(ICompleteGroup targetGroup)
        {
            bool isValid = true;

            var questions = targetGroup.GetAllQuestions<ICompleteQuestion>();
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                completeQuestion.Valid = Execute(completeQuestion);
                isValid = isValid && completeQuestion.Valid;
            }
            return isValid;
        }

        protected bool Execute(ICompleteQuestion question)
        {
            if (string.IsNullOrEmpty(question.ValidationExpression))
                return true;
            var e = new Expression(question.ValidationExpression);
            e.EvaluateParameter += (name, args) =>
                                       {
                                           Guid nameGuid = Guid.Parse(name);
                                           Guid? propagationKey = null;
                                          /* var propagation = question as PropagatableCompleteQuestion;
                                           if (propagation != null)
                                           {*/
                                        //   propagationKey = question.PropogationPublicKey;

                                         //  }
                                           args.Result =
                                               questionnaire.GetQuestionByKey(nameGuid, propagationKey).Answer;
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
    }
}
