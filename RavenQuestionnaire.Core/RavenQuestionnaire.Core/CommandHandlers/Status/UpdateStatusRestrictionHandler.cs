using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Status
{
    public class UpdateStatusRestrictionsHandler : ICommandHandler<UpdateStatusRestrictionsCommand>
    {
        private IStatusRepository _repository;


        public UpdateStatusRestrictionsHandler(IStatusRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(UpdateStatusRestrictionsCommand command)
        {
            Entities.Status entity = _repository.Load(command.StatusId);
            if (entity != null)
                entity.UpdateRestrictions(command.StatusRoles);
        }

    }
}
