using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateQuestionnaireCommand : ICommand
    {
        public string QuestionnaireId { get; set; }

        public string Title
        {
            get; set;
        }


        public UpdateQuestionnaireCommand(string questionnaireId, string title)
        {
           this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
           this.Title = title;
        }
    }
}
