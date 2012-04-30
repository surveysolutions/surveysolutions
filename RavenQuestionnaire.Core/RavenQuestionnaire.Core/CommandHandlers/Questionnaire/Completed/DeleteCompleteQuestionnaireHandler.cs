using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class DeleteCompleteQuestionnaireHandler:ICommandHandler<DeleteCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireUploaderService _completeQuestionnaireUploader;
        public DeleteCompleteQuestionnaireHandler(
            ICompleteQuestionnaireUploaderService completeQuestionnaireUploader)
        {
            this._completeQuestionnaireUploader = completeQuestionnaireUploader;
        }

        public void Handle(DeleteCompleteQuestionnaireCommand command)
        {
            this._completeQuestionnaireUploader.DeleteCompleteQuestionnaire(command.CompleteQuestionnaireId);

        }
    }
}
