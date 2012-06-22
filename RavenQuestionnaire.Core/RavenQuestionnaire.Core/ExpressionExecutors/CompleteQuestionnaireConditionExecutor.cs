using System;
using NCalc;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor// : IExpressionExecutor<CompleteQuestionnaire, bool>
    {
        private readonly GroupHash hash;
        public CompleteQuestionnaireConditionExecutor(GroupHash hash)
        {
            this.hash = hash;
        }
        public void Execute()
        {
            foreach (var completeQuestion in hash.Questions)
            {
              //  bool previousState = completeQuestion.Enabled;
                completeQuestion.Enabled = Execute(completeQuestion);
            }
        }

        public bool Execute(ICompleteQuestion question)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
                return true;
            var e = new Expression(question.ConditionExpression);
            e.EvaluateParameter += (name, args) =>
                                       {
                                           Guid nameGuid = Guid.Parse(name);
                                           Guid? propagationKey = question.PropogationPublicKey;
                                          /* var propagation = question;
                                           if (propagation != null)
                                           {*/
                                           //    propagationKey = propagation.PropogationPublicKey;

                                         //  }
                                           var value = hash[nameGuid, propagationKey].GetAnswerObject();
                                              // questionnaire.GetQuestionByKey(nameGuid, propagationKey).GetAnswerObject();
                                           if (value != null)
                                               args.Result = value;
                                           else
                                               args.Result = string.Empty;
//                                               questionnaire.GetQuestionByKey(nameGuid, propagationKey).GetValue();
                                       }
                ;
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
       /* protected ICompleteQuestion GetRegularQuestion(Guid target, ICompleteQuestion question, ICompleteGroup entity)
        {
            var dependency = entity.FirstOrDefault<ICompleteQuestion>(
                q => q.PublicKey.Equals(target) && !(q is IPropogate));
            return dependency;
        }*/
     /*   protected ICompleteQuestion GetPropagatedQuestion(Guid target, PropagatableCompleteQuestion question, ICompleteGroup entity)
        {
            //searchig for particulat question by key inside of all groups
            var dependencyPropagated =
                entity.GetPropagatedQuestion(target,
                                                    question.PropogationPublicKey);
            return dependencyPropagated;
        }*/
     /*   protected object GetValue(ICompleteQuestion question)
        {
            if (question == null)
                return null;
            var factory = new CompleteQuestionFactory();
            return factory.GetAnswerValue(question);

        }*/

    }
}
