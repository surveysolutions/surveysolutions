using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class GroupTemplateModel : ITemplateModel
    {
        public GroupTemplateModel(Guid id, string variableName, string? condition, string rosterScopeName, string parentScopeTypeName)
        {
            Id = id;
            VariableName = variableName;
            Condition = condition;
            RosterScopeName = rosterScopeName;
            ParentScopeTypeName = parentScopeTypeName;
        }

        public Guid Id { set; get; }
        public string VariableName { set; get; }

        public string? Condition { set; get; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }

        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.IdSuffix;
        public string ConditionMethodName => CodeGenerator.EnablementPrefix + VariableName;
    }
}
