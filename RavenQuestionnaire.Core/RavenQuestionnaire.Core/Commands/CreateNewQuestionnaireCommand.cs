namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewQuestionnaireCommand : ICommand
    {
        public string Title
        {
            get;
            private set;
        }


        public CreateNewQuestionnaireCommand(string title)
        {
            this.Title = title;
        }
    }
}
