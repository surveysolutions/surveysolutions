using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;


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
                questionnaire.UpdateGroup(command.GroupText, command.Paropagateble,command.Triggers, command.GroupPublicKey, command.ConditionExpression);
            else 
                questionnaire.UpdateGroup(command.GroupText, command.Paropagateble,command.GroupPublicKey, command.ConditionExpression);
        }
    }
}
