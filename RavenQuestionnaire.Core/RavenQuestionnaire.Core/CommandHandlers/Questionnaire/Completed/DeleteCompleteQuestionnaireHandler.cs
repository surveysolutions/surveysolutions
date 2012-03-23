using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class DeleteCompleteQuestionnaireHandler:ICommandHandler<DeleteCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireRepository _repository;
        public DeleteCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(DeleteCompleteQuestionnaireCommand command)
        {
            var entity = _repository.Load(command.CompleteQuestionnaireId);
            this._repository.Remove(entity);
        }
    }
}
