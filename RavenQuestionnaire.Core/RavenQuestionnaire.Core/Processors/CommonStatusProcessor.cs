using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Status.Processing;

namespace RavenQuestionnaire.Core.Processors
{
    public class CommonStatusProcessor
    {
        public void Process(CompleteQuestionnaireView completedQ, StatusProcessView status)
        {
            foreach (var item in status.FlowRules )
            {
              //exec condition and call change status 
              //item.ConditionExpression

            }

            if (status.DefaultIfNoConditions != null)
            {
                //call change status
            }

        }
    }
}
