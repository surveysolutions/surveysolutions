using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates
{
    public partial class InterviewExpressionStateTemplateV6
    {
        public InterviewExpressionStateTemplateV6(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV6 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV6(
                this.QuestionnaireStructure.QuestionnaireLevelModel, 
                this.QuestionnaireStructure.LookupTables);
        }

        protected RosterScopeTemplateV6 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV6(
                rosterGroup,
                this.QuestionnaireStructure.LookupTables);
        }
    }
}
