using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor// : IExpressionExecutor<CompleteQuestionnaire, bool>
    {
        private readonly CompleteQuestionnaire questionnaire;
        public CompleteQuestionnaireConditionExecutor(CompleteQuestionnaire questionnaire)
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
                                           var propagation = question as IPropogate;
                                           if (propagation == null)
                                           {
                                               var dependency = questionnaire.Find<ICompleteQuestion>(q => !(q is IPropogate) && q.PublicKey.Equals(nameGuid)).FirstOrDefault();
                                               var answer =
                                                   dependency.Find<ICompleteAnswer>(a => a.Selected).FirstOrDefault();
                                               args.Result = answer.AnswerValue ?? answer.AnswerText;
                                               return;
                                           }
                                         /*  var mainGroups =
                                               questionnaire.Find<ICompleteGroup>(
                                                   g => g.Propagated == Propagate.None && !(g is IPropogate));*/

                                           var mainGroups = questionnaire.Find<ICompleteGroup>(
                                               g =>
                                               g.Propagated == Propagate.None &&
                                               (g is ICompleteGroup<ICompleteGroup, ICompleteQuestion> &&
                                                ((ICompleteGroup<ICompleteGroup, ICompleteQuestion>) g).Questions.Count >
                                                0)).Select(
                                                    g => g as ICompleteGroup<ICompleteGroup, ICompleteQuestion>);


                                           var dependencyPropagated =
                                               mainGroups.SelectMany(g => g.Questions).FirstOrDefault(q => q.PublicKey.Equals(nameGuid));
                                             /*  Select(g => g.Find<ICompleteQuestion>(nameGuid)).FirstOrDefault(
                                                   q => q != null);*/
                                           if (dependencyPropagated == null)
                                           {
                                               var propagatedGroups =
                                                   questionnaire.Find<PropagatableCompleteGroup>(
                                                       g =>
                                                       g.PropogationPublicKey.Equals(propagation.PropogationPublicKey));
                                               dependencyPropagated = propagatedGroups
                                                   .Select(
                                                       g =>
                                                       g.Find<ICompleteQuestion>(nameGuid)).FirstOrDefault(g => g!=null);
                                               if (dependencyPropagated == null)
                                                   return;
                                           }
                                           var answerPropagated = dependencyPropagated.Find<ICompleteAnswer>(a => a.Selected).FirstOrDefault();
                                           args.Result = answerPropagated.AnswerValue ?? answerPropagated.AnswerText;
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

    }
}
