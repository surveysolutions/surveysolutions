using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class GroupTemplateModel : ITemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }

        public string Condition { set; get; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }

        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.IdSuffix;
        public string ConditionMethodName => CodeGenerator.EnablementPrefix + VariableName;
    }
}