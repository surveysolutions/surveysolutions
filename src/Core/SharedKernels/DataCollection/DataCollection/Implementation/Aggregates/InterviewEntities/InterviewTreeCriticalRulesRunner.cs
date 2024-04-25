#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeCriticalRulesRunner : IInterviewTreeCriticRulesRunner
    {
        private readonly IInterviewExpressionStorage expressionStorage;
        private readonly IQuestionnaire questionnaire;
        private readonly Identity questionnaireIdentity;

        public InterviewTreeCriticalRulesRunner(IInterviewExpressionStorage expressionStorage, IQuestionnaire questionnaire)
        {
            this.expressionStorage = expressionStorage;
            this.questionnaire = questionnaire;
            this.questionnaireIdentity = new Identity(questionnaire.QuestionnaireId, RosterVector.Empty);
        }

        public IEnumerable<Tuple<Guid, bool>> RunCriticalRules()
        {
            if (!questionnaire.DoesSupportCriticality())
                yield break;
            
            var interviewLevel = this.expressionStorage.GetLevel(this.questionnaireIdentity);

            if (interviewLevel is ICriticalRuleLevel criticalityConditionLevel)
            {
                var criticalRulesIds = questionnaire.GetCriticalRulesIds();
                foreach (var criticalRulesId in criticalRulesIds)
                {
                    var condition = criticalityConditionLevel.GetCriticalRule(criticalRulesId);
                    var runResult = RunConditionExpression(condition);
                    yield return new Tuple<Guid, bool>(criticalRulesId, runResult);
                }
            }
        }

        private static bool RunConditionExpression(Func<bool>? expression)
        {
            try
            {
                return expression == null || expression();
            }
            catch
            {
                return false;
            }
        }
    }
}
