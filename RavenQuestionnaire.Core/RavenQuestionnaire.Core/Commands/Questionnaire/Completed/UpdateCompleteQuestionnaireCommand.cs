using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class UpdateCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        
        public Guid? Status { get; private set; }
        public string Responsible { get; private set; }

        public UserLight Executor { get; set; }
        public string StatusHolderId { get; set; }

        public UpdateCompleteQuestionnaireCommand(string completeQuestionanireId, 
            Guid status,
            string statusHolderId,
            string responsible,
            UserLight executor)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            this.Status = status;
            this.Responsible = responsible;
            this.Executor = executor;
            StatusHolderId = IdUtil.CreateStatusId(statusHolderId);
        }
    }
}
