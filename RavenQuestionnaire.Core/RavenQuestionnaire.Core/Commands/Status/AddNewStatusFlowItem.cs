using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Status
{
    public class AddNewStatusFlowItem : ICommand
    {
        public string Status { get; set; }

        public string ChangeRule { set; get; }
        public string ChangeComment { set; get; }

        public Guid PublicKey { get; set; }

        public SurveyStatus TargetStatus { set; get; }

        public AddNewStatusFlowItem(string status, string changeRule, string changeComment, SurveyStatus targetStatus, Guid publicKey, UserLight executor)
        {
            ChangeRule = changeRule;
            ChangeComment = changeComment;
            TargetStatus = targetStatus;
            Status = IdUtil.CreateStatusId(status); 
            Executor = executor;
            PublicKey = publicKey;
        }

        public UserLight Executor
        {
            get ;set ;
        }
    }
}
