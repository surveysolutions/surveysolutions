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
    public class InterviewTreeCriticalRulesRunner : IInterviewTreeCriticalityRunner, IDisposable
    {
        private readonly IInterviewExpressionStorage expressionStorage;
        private readonly IQuestionnaire questionnaire;
        private readonly Identity questionnaireIdentity;

        private readonly ConcurrentDictionary<Identity, IInterviewLevel> memoryCache = new ConcurrentDictionary<Identity, IInterviewLevel>();

        public InterviewTreeCriticalRulesRunner(IInterviewExpressionStorage expressionStorage, IQuestionnaire questionnaire)
        {
            this.expressionStorage = expressionStorage;
            this.questionnaire = questionnaire;
            this.questionnaireIdentity = new Identity(questionnaire.QuestionnaireId, RosterVector.Empty);
        }

        public IEnumerable<Tuple<Guid, bool>> RunCriticalRules()
        {
            var interviewLevel = this.expressionStorage.GetLevel(this.questionnaireIdentity);
            var rootLevel = this.memoryCache.GetOrAdd(this.questionnaireIdentity, interviewLevel);

            if (rootLevel is ICriticalRuleLevel criticalityConditionLevel)
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
            catch (TypeLoadException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            this.memoryCache.Clear();
        }
    }
}
