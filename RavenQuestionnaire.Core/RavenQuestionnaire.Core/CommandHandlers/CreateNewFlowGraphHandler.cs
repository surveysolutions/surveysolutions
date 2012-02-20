using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
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
