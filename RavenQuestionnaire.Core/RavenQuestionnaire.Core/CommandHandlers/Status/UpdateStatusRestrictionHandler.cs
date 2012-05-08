using System.Linq;
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
            {
                var currentStatus = entity.GetInnerDocument().Statuses.FirstOrDefault(x => x.PublicKey == command.PublicKey);
                if (currentStatus != null)
                {
                    currentStatus.StatusRoles = command.StatusRoles;
                }
            }
        }

    }
}
