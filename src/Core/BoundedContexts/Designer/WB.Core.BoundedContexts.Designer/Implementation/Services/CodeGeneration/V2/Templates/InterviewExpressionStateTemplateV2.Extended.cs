using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V2.Templates
{
    public partial class InterviewExpressionStateTemplateV2
    {
        public InterviewExpressionStateTemplateV2(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV2 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV2(this.QuestionnaireStructure.QuestionnaireLevelModel);
        }

        protected RosterScopeTemplateV2 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV2(rosterGroup);
        }
    }
}
