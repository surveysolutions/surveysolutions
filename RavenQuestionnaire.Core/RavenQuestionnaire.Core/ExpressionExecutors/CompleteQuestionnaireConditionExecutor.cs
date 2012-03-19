using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor// : IExpressionExecutor<CompleteQuestionnaire, bool>
    {
        private readonly ICompleteGroup questionnaire;
        public CompleteQuestionnaireConditionExecutor(ICompleteGroup questionnaire)
        {
            this.questionnaire = questionnaire;
        }

        public bool Execute(ICompleteQuestion question)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
                return true;
            var e = new Expression(question.ConditionExpression);
            e.EvaluateParameter += (name, args) =>
                                       {
                                           Guid nameGuid = Guid.Parse(name);
                                           var propagation = question as PropagatableCompleteQuestion;
                                           if (propagation == null)
                                           {
                                               args.Result =
                                                   GetValue(GetRegularQuestion(nameGuid, question, questionnaire));
                                               return;
                                           }
                                           args.Result =
                                               GetValue(GetPropagatedQuestion(nameGuid, propagation, questionnaire) ??
                                                        GetRegularQuestion(nameGuid, question, questionnaire));
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
        protected ICompleteQuestion GetRegularQuestion(Guid target, ICompleteQuestion question, ICompleteGroup entity)
        {
            var dependency = entity.FirstOrDefault<ICompleteQuestion>(
                q => q.PublicKey.Equals(target) && !(q is IPropogate));
            return dependency;
        }
        protected ICompleteQuestion GetPropagatedQuestion(Guid target, PropagatableCompleteQuestion question, ICompleteGroup entity)
        {
            //searchig for particulat question by key inside of all groups
            var dependencyPropagated =
                entity.GetPropagatedQuestion(target,
                                                    question.PropogationPublicKey);
            return dependencyPropagated;
        }
        protected object GetValue(ICompleteQuestion question)
        {
            if (question == null)
                return null;
            var factory = new CompleteQuestionFactory();
            return factory.GetAnswerValue(question);

        }

    }
}
