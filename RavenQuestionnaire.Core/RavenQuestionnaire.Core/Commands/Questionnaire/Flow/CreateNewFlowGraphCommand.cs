using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Flow
{
    public class CreateNewFlowGraphCommand: ICommand
    {
        public Entities.Questionnaire Questionnaire
        {
            get;
            private set;
        }

        public UserLight Executor { get; set; }

        public CreateNewFlowGraphCommand(Entities.Questionnaire questionnaire, UserLight executor)
        {
            this.Questionnaire = questionnaire;
            Executor = executor;

        }
    }
}
