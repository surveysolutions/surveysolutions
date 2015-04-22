using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class InterviewExpressionStateTemplate
    {
        public InterviewExpressionStateTemplate(QuestionnaireExecutorTemplateModel questionnaire)
        {
            this.QuestionnaireTemplateStructure = questionnaire;
        }

        public QuestionnaireExecutorTemplateModel QuestionnaireTemplateStructure { get; private set; }

        public QuestionnaireLevelTemplate CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplate(QuestionnaireTemplateStructure.QuestionnaireLevelModel);
        }

        protected RosterScopeTemplate CreateRosterScopeTemplate(KeyValuePair<string, List<RosterTemplateModel>> rosterGroup)
        {
            return new RosterScopeTemplate(rosterGroup, QuestionnaireTemplateStructure);
        }
    }
}
