using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group
{
    public class CreateNewGroupHandler : ICommandHandler<CreateNewGroupCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public CreateNewGroupHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(CreateNewGroupCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);

            questionnaire.AddGroup(command.GroupText,command.PublicKey, command.Paropagateble, command.ParentGroupPublicKey);
        }
    }
}
