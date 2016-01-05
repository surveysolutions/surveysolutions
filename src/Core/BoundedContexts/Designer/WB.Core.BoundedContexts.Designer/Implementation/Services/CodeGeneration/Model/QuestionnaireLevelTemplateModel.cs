using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireLevelTemplateModel : RosterScopeBaseModel
    {
        public QuestionnaireLevelTemplateModel()
            : base(
            "@__questionnaire_scope", 
            "QuestionnaireTopLevel",
            new List<GroupTemplateModel>(), 
            new List<QuestionTemplateModel>(), 
            new List<RosterTemplateModel>(), 
            new List<Guid>(),
            null)
        {
        }

        public List<ConditionMethodAndState> ConditionMethodsSortedByExecutionOrder { get; set; }
    }
}