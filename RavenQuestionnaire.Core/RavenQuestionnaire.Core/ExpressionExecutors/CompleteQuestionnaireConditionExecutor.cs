using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
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
                                           var entity = questionnaire.GetInnerDocument();
                                           var dependency = entity.GetAllQuestions().Where(
                                               q => !(q is IPropogate)).FirstOrDefault(
                                                   q => q.PublicKey.Equals(nameGuid));

                                           if (dependency != null)
                                           {
                                               var answer =
                                                   dependency.Find<ICompleteAnswer>(a => a.Selected).FirstOrDefault();
                                               if (answer == null)
                                               {
                                                   args.Result = string.Empty;
                                                   return;
                                               }
                                               //question wasn't propagated so we looking for only main question
                                               args.Result = answer.AnswerValue ?? answer.AnswerText;
                                               return;
                                           }
                                           /* question was propagated*/
                                           var propagation = question as IPropogate;
                                           if (propagation == null)
                                           {
                                               return;
                                           }
                                           //searchig for particulat question by key inside of all groups
                                           var dependencyPropagated =
                                               entity.GetPropagatedGroupsByKey(propagation.PropogationPublicKey).
                                                   SelectMany(pg => pg.GetAllQuestions()).FirstOrDefault(
                                                       q => q.PublicKey.Equals(nameGuid));
                                           if (dependencyPropagated == null)
                                               return;

                                           var answerPropagated =
                                               dependencyPropagated.Find<ICompleteAnswer>(a => a.Selected).
                                                   FirstOrDefault();
                                           if (answerPropagated == null)
                                           {
                                               args.Result = string.Empty;
                                               return;
                                           }
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
