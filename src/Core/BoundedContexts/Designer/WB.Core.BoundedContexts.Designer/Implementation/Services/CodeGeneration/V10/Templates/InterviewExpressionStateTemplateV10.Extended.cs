using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates
{
    public partial class InterviewExpressionStateTemplateV10
    {
        public InterviewExpressionStateTemplateV10(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV10 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV10(
                this.QuestionnaireStructure.QuestionnaireLevelModel, 
                this.QuestionnaireStructure.LookupTables);
        }

        protected RosterScopeTemplateV10 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV10(
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
