using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class CreateNewCompleteQuestionnaireHandler : ICommandHandler<CreateNewCompleteQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private ICompleteQuestionnaireUploaderService _completeQuestionnaireUploader;
        
        public CreateNewCompleteQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository, ICompleteQuestionnaireUploaderService completeQuestionnaireUploader)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._completeQuestionnaireUploader = completeQuestionnaireUploader;
        }

        public void Handle(CreateNewCompleteQuestionnaireCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);
            this._completeQuestionnaireUploader.AddCompleteAnswer(questionnaire, command.CompleteAnswers, command.UserId);
        }
    }
}
