using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class UpdateAnswerInCompleteQuestionnaireHandler :
        ICommandHandler<UpdateAnswerInCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireUploaderService _completeQuestionnaireUploader;

        public UpdateAnswerInCompleteQuestionnaireHandler(
            ICompleteQuestionnaireUploaderService completeQuestionnaireUploader)
        {
            this._completeQuestionnaireUploader = completeQuestionnaireUploader;
        }

        public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            this._completeQuestionnaireUploader.AddCompleteAnswer(command.CompleteQuestionnaireId,
                                                                  command.QuestionPublickey, command.Propagationkey,
                                                                  command.CompleteAnswer ?? command.CompleteAnswers);
        }
    }
}
