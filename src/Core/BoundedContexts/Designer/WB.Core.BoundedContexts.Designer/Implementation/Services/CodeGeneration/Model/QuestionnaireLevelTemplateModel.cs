using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireLevelTemplateModel : RosterScopeBaseModel
    {
        public QuestionnaireLevelTemplateModel()
            : base(
            CodeGenerator.QuestionnaireScope, 
            CodeGenerator.QuestionnaireTypeName,
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