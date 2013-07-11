using System.Collections.Generic;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.ExpressionExecutors
{
    public interface ICompleteQuestionnaireConditionExecuteCollector {
       
        void CollectGroupHierarhicallyStates(ICompleteGroup completeGroup, bool parentState, Dictionary<string, bool?> resultGroupStatus, Dictionary<string, bool?> resultQuestionsStatus);

        void ExecuteConditionAfterAnswer(ICompleteQuestion question, Dictionary<string, bool?> resultQuestionsStatus, Dictionary<string, bool?> resultGroupsStatus);
    }
}