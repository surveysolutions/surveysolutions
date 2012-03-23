using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Flow;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateQuestionnaireFlowHandler : ICommandHandler<UpdateQuestionnaireFlowCommand>
    {
        private readonly IFlowGraphRepository _repository;

        public UpdateQuestionnaireFlowHandler(IFlowGraphRepository repository)
        {
            _repository = repository;
        }

        #region ICommandHandler<UpdateQuestionnaireFlowCommand> Members

        public void Handle(UpdateQuestionnaireFlowCommand command)
        {
            var flowGraph = _repository.Load(command.FlowGraphId);
            
            flowGraph.UpdateFlow(command.Blocks, command.Connections);
        }

        #endregion
    }
}