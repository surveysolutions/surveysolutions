using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeleteQuestionnaireHandler: ICommandHandler<DeleteQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public DeleteQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(DeleteQuestionnaireCommand command)
        {
            var entity = _questionnaireRepository.Load(command.QuestionnaireId);
            this._questionnaireRepository.Remove(entity);
        }
    }
}
