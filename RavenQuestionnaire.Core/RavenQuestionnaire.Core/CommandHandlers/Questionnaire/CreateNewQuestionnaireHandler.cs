using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    public class CreateNewQuestionnaireHandler : ICommandHandler<CreateNewQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IFlowGraphRepository _flowGraphRepository;

        public CreateNewQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository, 
            IFlowGraphRepository flowGraphRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._flowGraphRepository = flowGraphRepository;
        }

        public void Handle(CreateNewQuestionnaireCommand command)
        {
            var newQuestionnaire = new Entities.Questionnaire(command.Title);
            _questionnaireRepository.Add(newQuestionnaire);

            _flowGraphRepository.Add(new FlowGraph(newQuestionnaire.QuestionnaireId));
        }
    }
}
