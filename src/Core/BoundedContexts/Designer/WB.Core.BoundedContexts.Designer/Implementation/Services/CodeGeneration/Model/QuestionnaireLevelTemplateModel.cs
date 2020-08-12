using System;
using System.Collections.Generic;
using System.Linq;

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
            new List<StaticTextTemplateModel>(), 
            new List<RosterTemplateModel>(), 
            new List<Guid>(),
            new List<VariableTemplateModel>(), 
            null)
        {
            LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter = new List<LinkedQuestionVerifierModel>();
        }

        public List<QuestionTemplateModel> QuestionsWithOptionsFilter => Questions.Where(x => x.HasOptionsFilter).ToList();

        public List<ConditionMethodAndState> ConditionMethodsSortedByExecutionOrder { get; set; } = new List<ConditionMethodAndState>();
        public List<LinkedQuestionVerifierModel> LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter { get; set; }
    }
}
