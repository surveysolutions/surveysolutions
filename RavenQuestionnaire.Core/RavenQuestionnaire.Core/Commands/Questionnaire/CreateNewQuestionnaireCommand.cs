using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class CreateNewQuestionnaireCommand : ICommand
    {
        public string Title
        {
            get;
            private set;
        }

        public CreateNewQuestionnaireCommand(string title, UserLight executor)
        {
            this.Title = title;
            this.Executor = executor;
        }


        public UserLight Executor
        {
            get;
            set;
        }
    }
}
