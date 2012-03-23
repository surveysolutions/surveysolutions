using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class UpdateCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        
        public SurveyStatus Status { get; private set; }
        public UserLight Responsible { get; private set; }

        public UserLight Executor { get; set; }
        public string ChangeComment { get; set; }

        public UpdateCompleteQuestionnaireCommand(string completeQuestionanireId, 
            SurveyStatus statusId,
            string changeComment,
            UserLight responsible,
            UserLight executor)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            this.Status = statusId;
            this.Responsible = responsible;
            this.Executor = executor;
            ChangeComment = changeComment;
        }
    }
}
