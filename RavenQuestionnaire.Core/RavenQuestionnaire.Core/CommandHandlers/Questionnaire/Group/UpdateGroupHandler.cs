using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group
{
    public class UpdateGroupHandler : ICommandHandler<UpdateGroupCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public UpdateGroupHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(UpdateGroupCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);

            if (command.Triggers!=null)
                questionnaire.UpdateGroup(command.GroupText, command.Paropagateble,command.Triggers, command.GroupPublicKey);
            else 
                questionnaire.UpdateGroup(command.GroupText, command.Paropagateble,command.GroupPublicKey);
        }
    }
}
