using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class CreateNewQuestionnaireHandler : ICommandHandler<CreateNewQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public CreateNewQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(CreateNewQuestionnaireCommand command)
        {
            Questionnaire newUser = new Questionnaire(command.Title);
            _questionnaireRepository.Add(newUser);
        }
    }
}
