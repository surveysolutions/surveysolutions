using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Status
{
    public class AddNewStatusFlowItem : ICommand
    {
        public string Status { get; set; }

        public string ChangeRule { set; get; }
        public string ChangeComment { set; get; }

        public SurveyStatus TargetStatus { set; get; }

        public AddNewStatusFlowItem(string status, string changeRule, string changeComment, SurveyStatus targetStatus, UserLight executor)
        {
            ChangeRule = changeRule;
            ChangeComment = changeComment;
            TargetStatus = targetStatus;
            Status = status;
            Executor = executor;
        }

        public UserLight Executor
        {
            get ;set ;
        }
    }
}
