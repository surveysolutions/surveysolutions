using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class DeleteCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; set; }
        public UserLight Executor { get; set; }

        public DeleteCompleteQuestionnaireCommand(string completeQeuestionnaireId, UserLight executor)
        {
            this.CompleteQuestionnaireId = completeQeuestionnaireId;
            Executor = executor;
        }
    }
}
