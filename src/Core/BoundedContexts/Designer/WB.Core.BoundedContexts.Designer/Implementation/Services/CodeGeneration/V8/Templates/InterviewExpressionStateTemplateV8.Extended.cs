using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V8.Templates
{
    public partial class InterviewExpressionStateTemplateV8
    {
        public InterviewExpressionStateTemplateV8(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV8 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV8(
                this.QuestionnaireStructure.QuestionnaireLevelModel, 
                this.QuestionnaireStructure.LookupTables);
        }

        protected RosterScopeTemplateV8 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV8(
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
