using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class DeleteQuestionnaireCommand : ICommand
    {
        
        public string QuestionnaireId { get; set; }

        public UserLight Executor { get; set; }

        public DeleteQuestionnaireCommand(string questionnaireId, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            Executor = executor;
        }
    }
}
