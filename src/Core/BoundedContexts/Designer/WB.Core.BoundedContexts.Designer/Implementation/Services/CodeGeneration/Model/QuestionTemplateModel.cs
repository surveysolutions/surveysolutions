using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionTemplateModel : ITemplateModel
    {
        public QuestionTemplateModel(Guid id, string rosterScopeName, string typeName, string variableName,
            string parentScopeTypeName)
        {
            Id = id;
            TypeName = typeName;
            VariableName = variableName;
            ParentScopeTypeName = parentScopeTypeName;
            RosterScopeName = rosterScopeName;

            ValidationExpressions = new List<ValidationExpressionModel>();
            AllMultioptionYesNoCodes = new List<string>();
        }

        public Guid Id { set; get; }
        public string VariableName { set; get; }

        public List<GeneratedVariable> GeneratedVariables { get; set; } = new List<GeneratedVariable>();

        public string? Condition { set; get; }

        public List<ValidationExpressionModel> ValidationExpressions { get; set; }

        public string TypeName { set; get; }
        public string ConditionMethodName => CodeGeneratorV2.EnablementPrefix + this.VariableName;
        public string OptionsFilterMethodName => CodeGeneratorV2.OptionsFilterPrefix + this.VariableName;

        public string MemberName => CodeGeneratorV2.PrivateFieldsPrefix + VariableName;
        public string StateName => CodeGeneratorV2.PrivateFieldsPrefix + VariableName + CodeGeneratorV2.StateSuffix;
        public string IdName => CodeGeneratorV2.GetQuestionIdName(VariableName);

        public bool IsMultiOptionYesNoQuestion { get; set; }
        public List<string> AllMultioptionYesNoCodes { get; set; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }
        public string? OptionsFilterExpression { get; set; }

        public bool HasOptionsFilter => !string.IsNullOrWhiteSpace(this.OptionsFilterExpression);
    }
}
