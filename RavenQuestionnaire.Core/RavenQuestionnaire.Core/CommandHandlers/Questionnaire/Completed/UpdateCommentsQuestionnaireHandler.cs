using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;


namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class UpdateCommentsQuestionnaireHandler : ICommandHandler<UpdateCommentsInCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireUploaderService _completeQuestionnaireUploader;

        public UpdateCommentsQuestionnaireHandler(ICompleteQuestionnaireUploaderService completeQuestionnaireUploader)
        {
            this._completeQuestionnaireUploader = completeQuestionnaireUploader;
        }

        public void Handle(UpdateCommentsInCompleteQuestionnaireCommand command)
        {
            this._completeQuestionnaireUploader.AddComments(command.CompleteQuestionnaireId, 
                                                command.QuestionPublickey, command.Propagationkey,
                                                command.Comments);
        }
    }
}