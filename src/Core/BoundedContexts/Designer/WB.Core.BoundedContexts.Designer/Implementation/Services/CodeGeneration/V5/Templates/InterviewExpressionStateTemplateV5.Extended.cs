using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates
{
    public partial class InterviewExpressionStateTemplateV5
    {
        public InterviewExpressionStateTemplateV5(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV5 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV5(
                this.QuestionnaireStructure.QuestionnaireLevelModel, 
                this.QuestionnaireStructure.LookupTables);
        }

        protected RosterScopeTemplateV5 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV5(
                rosterGroup,
                this.QuestionnaireStructure.LookupTables);
        }
    }
}
