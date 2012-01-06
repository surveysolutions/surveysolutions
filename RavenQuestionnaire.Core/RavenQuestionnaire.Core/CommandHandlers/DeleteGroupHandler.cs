using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeleteGroupHandler : ICommandHandler<DeleteGroupCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public DeleteGroupHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(DeleteGroupCommand command)
        {
            var entity = _questionnaireRepository.Load(command.QuestionnaireId);
            entity.Remove<Group>(command.GroupPublicKey);
            //  this._questionRepository.Remove(entity);
            //  entity.
        }
    }
}
