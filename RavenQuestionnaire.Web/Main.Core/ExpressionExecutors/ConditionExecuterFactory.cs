using System;
using System.Runtime.CompilerServices;
using Main.Core.Documents;

namespace Main.Core.ExpressionExecutors
{
    public static class ConditionExecuterFactory
    {
        public static Func<CompleteQuestionnaireDocument, ICompleteQuestionnaireConditionExecuteCollector> Creator = doc => new CompleteQuestionnaireConditionExecuteCollector(doc);

        public static ICompleteQuestionnaireConditionExecuteCollector GetConditionExecuter(CompleteQuestionnaireDocument completeQuestionnaire)
        {
            return Creator(completeQuestionnaire);
        }
    }
}