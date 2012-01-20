using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Views.Status.Processing;

namespace RavenQuestionnaire.Core.Processors
{
    /// <summary>
    /// Processes CQ according to the rules
    /// </summary>
    public class CommonStatusProcessor
    {
        private ICommandInvoker _commandInvoker;
        private const string DefaultStatusChangeComment = "No condition was succeded.";

        public CommonStatusProcessor(ICommandInvoker commandInvoker)
        {
            _commandInvoker = commandInvoker;
        }

        public void Process(CompleteQuestionnaire completedQ, StatusProcessView status)
        {
            StatusProcessorExecutor executor= new StatusProcessorExecutor();
            
            //iterates over all rules and get first true condition
            //order of items is important
            foreach (var item in status.FlowRules)
            {
                if (executor.Execute(completedQ, item.ConditionExpression))
                {
                    CallStatusChange(completedQ.CompleteQuestinnaireId, item.StatusToGo, item.ChangeComment);
                    return;
                }
            }

            //any condition was not rised
            if (status.DefaultIfNoConditions != null)
            {
                CallStatusChange(completedQ.CompleteQuestinnaireId, 
                    status.DefaultIfNoConditions, 
                    DefaultStatusChangeComment);
            }
        }


        private void CallStatusChange(string completeQuestionanireId, SurveyStatus newStatus, string changeComment)
        {
            _commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(completeQuestionanireId,
                    newStatus,
                    changeComment,
                    null,
                    new UserLight("-1", "system")));
        }
    }
}
