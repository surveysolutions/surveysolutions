using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Status.Processing;

namespace RavenQuestionnaire.Core.Processors
{
    /// <summary>
    /// Processes CQ according to the rules
    /// </summary>
    public class CommonStatusProcessor
    {
        public void Process(CompleteQuestionnaire completedQ, StatusProcessView status)
        {
            StatusProcessorExecutor executor= new StatusProcessorExecutor();
            //iterate over all rules and get first true condition
            //order of items is important
            foreach (var item in status.FlowRules )
            {
                if (executor.Execute(completedQ, item.ConditionExpression))
                {
                    //call change status 
                    return;
                }

            }

            if (status.DefaultIfNoConditions != null)
            {
                //call change status
            }
        }
        private void CallStatusChange()
        {

        }
    }
}
