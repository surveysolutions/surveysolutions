using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V9.Templates
{
    public partial class InterviewExpressionStateTemplateV9
    {
        public InterviewExpressionStateTemplateV9(QuestionnaireExpressionStateModel questionnaire)
        {
            this.QuestionnaireStructure = questionnaire;
        }

        public QuestionnaireExpressionStateModel QuestionnaireStructure { get; private set; }

        public QuestionnaireLevelTemplateV9 CreateQuestionnaireLevelTemplate()
        {
            return new QuestionnaireLevelTemplateV9(
                this.QuestionnaireStructure.QuestionnaireLevelModel, 
                this.QuestionnaireStructure.LookupTables);
        }

        protected RosterScopeTemplateV9 CreateRosterScopeTemplate(RosterScopeTemplateModel rosterGroup)
        {
            return new RosterScopeTemplateV9(
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
