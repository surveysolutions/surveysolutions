using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Status
{
    public class CreateStatusFlowHandler : ICommandHandler<AddNewStatusFlowItem>
    {
        private IStatusRepository _repository;
        
        public CreateStatusFlowHandler (IStatusRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(AddNewStatusFlowItem command)
        {
            Entities.Status status = this._repository.Load(command.Status);

            if (status != null)
            {
                //status.AddFlowRule(new FlowRule(command.ChangeRule, command.ChangeComment, command.TargetStatus));
            }
        }
    }
}
