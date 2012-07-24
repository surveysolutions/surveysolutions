using NCalc;
using System;
using System.Linq;
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

        public void Execute(ICompleteGroup group)
        {
            foreach (ICompleteQuestion completeQuestion in group.Children.Where(c => c is ICompleteQuestion))
            {
                completeQuestion.Valid = Execute(completeQuestion);
            }
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
            string expression = question.ValidationExpression.ToLower();
            if (expression.Contains("this"))
                expression = expression.Replace("this", question.PublicKey.ToString());
            var e = new Expression(expression);
            e.EvaluateParameter += (name, args) =>
                                       {
                                           Guid nameGuid = Guid.Parse(name);
                                           Guid? propagationKey = question.PropogationPublicKey;
                                           var value = hash[nameGuid, propagationKey].GetAnswerObject();
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
    }
}
