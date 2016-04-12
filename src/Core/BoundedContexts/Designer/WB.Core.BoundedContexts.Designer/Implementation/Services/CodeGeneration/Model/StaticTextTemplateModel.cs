using System;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class StaticTextTemplateModel : ITemplateModel
    {
        public StaticTextTemplateModel(Guid id, string condition, string rosterScopeName, string parentScopeTypeName)
        {
            this.Id = id;
            this.Condition = condition;
            this.RosterScopeName = rosterScopeName;
            this.ParentScopeTypeName = parentScopeTypeName;
        }

        public Guid Id { get; }

        public string Condition { get; }

        public string RosterScopeName { get; }
        public string ParentScopeTypeName { get; }

        public string VariableName => this.Id.FormatGuid();
        public string StateName => CodeGenerator.PrivateFieldsPrefix + this.VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.PrivateFieldsPrefix + this.VariableName + CodeGenerator.IdSuffix;
        public string ConditionMethodName => CodeGenerator.EnablementPrefix + this.VariableName;
    }
}