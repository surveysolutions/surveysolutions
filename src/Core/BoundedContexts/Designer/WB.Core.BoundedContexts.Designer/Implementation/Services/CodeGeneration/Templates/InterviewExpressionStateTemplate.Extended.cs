using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Versions;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class InterviewExpressionStateTemplate
    {
        public InterviewExpressionStateTemplate(QuestionnaireExecutorTemplateModel questionnaire, IVersionParameters versionParameters)
        {
            this.QuestionnaireTemplateStructure = questionnaire;
            this.VersionParameters = versionParameters;
        }

        protected QuestionnaireExecutorTemplateModel QuestionnaireTemplateStructure { get; private set; }
        protected IVersionParameters VersionParameters { get; private set; }

        protected QuestionnaireLevelTemplate CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplate(QuestionnaireTemplateStructure.QuestionnaireLevelModel, this.VersionParameters);
        }

        protected RosterScopeTemplate CreateRosterScopeTemplate(KeyValuePair<string, List<RosterTemplateModel>> rosterGroup)
        {
            return new RosterScopeTemplate(rosterGroup, QuestionnaireTemplateStructure, this.VersionParameters);
        }
    }
}
