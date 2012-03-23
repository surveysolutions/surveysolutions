using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Location
{
    public class CreateNewLocationCommand: ICommand
    {
        public string Location
        {
            get;
            private set;
        }

        public UserLight Executor { get; set; }

        public CreateNewLocationCommand(string location, UserLight executor)
        {
            this.Location = location;
            Executor = executor;

        }
    }
}
