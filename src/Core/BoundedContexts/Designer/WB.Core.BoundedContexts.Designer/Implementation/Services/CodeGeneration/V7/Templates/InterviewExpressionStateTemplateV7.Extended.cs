using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V7.Templates
{
    public partial class InterviewExpressionStateTemplateV7
    {
        public InterviewExpressionStateTemplateV7(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV7 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV7(
                this.QuestionnaireStructure.QuestionnaireLevelModel, 
                this.QuestionnaireStructure.LookupTables);
        }

        protected RosterScopeTemplateV7 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV7(
                rosterGroup,
                this.QuestionnaireStructure.LookupTables);
        }

        protected Dictionary<Guid, Guid> GetLinkedQuestionIdWithSourceRosterIdPairs()
        {
            return
                this.QuestionnaireStructure.AllRosters.SelectMany(r => r.LinkedQuestionFilterExpressions)
                    .ToDictionary(l => l.LinkedQuestionId, l => l.RosterId);
        }
    }
}
