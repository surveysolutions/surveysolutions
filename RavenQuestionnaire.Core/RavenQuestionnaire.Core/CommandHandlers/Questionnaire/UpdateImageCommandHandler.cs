using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    public class UpdatemageCommandHandler : ICommandHandler<UpdateImageCommand>
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        
        public UpdatemageCommandHandler(IQuestionnaireRepository questionnaireRepository)
        {
            _questionnaireRepository = questionnaireRepository;
        }
        public void Handle(UpdateImageCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(IdUtil.CreateQuestionnaireId(command.QuestionnaireId));

            var question = questionnaire.Find<Entities.SubEntities.Question>(command.QuestionKey);

            question.UpdateCard(command.ImageKey, command.Title, command.Description);
       }
    }
}
