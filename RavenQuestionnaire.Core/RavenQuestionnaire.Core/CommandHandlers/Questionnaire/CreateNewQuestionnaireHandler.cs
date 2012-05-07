using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    public class CreateNewQuestionnaireHandler : ICommandHandler<CreateNewQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IFlowGraphRepository _flowGraphRepository;
        private IStatusRepository _statusRepository;

        public CreateNewQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository,
            IFlowGraphRepository flowGraphRepository, IStatusRepository statusRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._flowGraphRepository = flowGraphRepository;
            this._statusRepository = statusRepository;
        }

        public void Handle(CreateNewQuestionnaireCommand command)
        {
            var newQuestionnaire = new Entities.Questionnaire(command.Title);
            _questionnaireRepository.Add(newQuestionnaire);
            _flowGraphRepository.Add(new FlowGraph(newQuestionnaire.QuestionnaireId));

            var status = new Entities.Status(newQuestionnaire.QuestionnaireId);

            //copy default statuses
            if (command.DefaultStatusGroupId != null)
            {
                var statusDefault = _statusRepository.Load(command.DefaultStatusGroupId);
                if (statusDefault != null)
                    status.GetInnerDocument().Statuses = statusDefault.GetInnerDocument().Statuses;
            }
            _statusRepository.Add(status);
        }
    }
}
