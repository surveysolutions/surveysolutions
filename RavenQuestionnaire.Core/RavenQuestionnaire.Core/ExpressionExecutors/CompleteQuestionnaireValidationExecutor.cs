using System;
using NCalc;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireValidationExecutor
    {
        private readonly GroupHash hash;
        public CompleteQuestionnaireValidationExecutor(GroupHash hash)
        {
            this.hash = hash;
        }
        public bool Execute()
        {
            bool isValid = true;

            foreach (ICompleteQuestion completeQuestion in hash.Questions)
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
                                           Guid? propagationKey = question.PropogationPublicKey;

                                           var value = hash[nameGuid, propagationKey].GetAnswerObject();
                                           if (value != null)
                                               args.Result = value;
                                           else
                                               args.Result = string.Empty;
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
