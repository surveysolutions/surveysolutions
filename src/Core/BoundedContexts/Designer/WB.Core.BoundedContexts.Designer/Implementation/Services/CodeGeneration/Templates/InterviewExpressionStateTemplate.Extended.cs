using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class InterviewExpressionStateTemplate
    {
        public InterviewExpressionStateTemplate(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplate CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplate(this.QuestionnaireStructure.QuestionnaireLevelModel);
        }

        protected RosterScopeTemplate CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplate(rosterGroup);
        }
    }
}
