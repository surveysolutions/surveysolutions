using RavenQuestionnaire.Core.Commands.Questionnaire.Flow;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Flow
{
    public class CreateNewFlowGraphHandler : ICommandHandler<CreateNewFlowGraphCommand>
    {
        private IFlowGraphRepository _repository;
        public CreateNewFlowGraphHandler(IFlowGraphRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(CreateNewFlowGraphCommand command)
        {
            FlowGraph entity = new FlowGraph(command.Questionnaire);
            this._repository.Add(entity);
        }
    }
}
