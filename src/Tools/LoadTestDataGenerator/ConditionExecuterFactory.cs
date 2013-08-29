using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.ExpressionExecutors;

namespace LoadTestDataGenerator
{
    internal class FakeCompleteQuestionnaireConditionExecuteCollector : ICompleteQuestionnaireConditionExecuteCollector
    {
        public void CollectGroupHierarhicallyStates(ICompleteGroup completeGroup, bool parentState, Dictionary<string, bool?> resultGroupStatus,
                                                    Dictionary<string, bool?> resultQuestionsStatus)
        {
            
        }

        public void ExecuteConditionAfterAnswer(ICompleteQuestion question, Dictionary<string, bool?> resultQuestionsStatus,
                                                Dictionary<string, bool?> resultGroupsStatus)
        {
           
        }
    }
}
