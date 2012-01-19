using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        
        public SurveyStatus Status { get; private set; }
        public UserLight Responsible { get; private set; }

        public UserLight Executor { get; set; }


        public UpdateCompleteQuestionnaireCommand(string completeQuestionanireId, 
            SurveyStatus statusId, 
            UserLight responsible,
            UserLight executor)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            this.Status = statusId;
            this.Responsible = responsible;
            this.Executor = executor;
        }
    }
}
