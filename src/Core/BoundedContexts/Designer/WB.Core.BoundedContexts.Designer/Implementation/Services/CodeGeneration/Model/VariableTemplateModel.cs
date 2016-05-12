using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class VariableTemplateModel
    {
        public VariableTemplateModel(Guid id, string variableName, string expression, string typeName, string rosterScopeName, string parentScopeTypeName)
        {
            this.Id = id;
            this.VariableName = variableName;
            this.Expression = expression;
            this.TypeName = typeName;
            this.RosterScopeName = rosterScopeName;
            this.ParentScopeTypeName = parentScopeTypeName;
        }

        public Guid Id { get; }
        public string VariableName { get; }
        public string Expression { get; }
        public string TypeName { get; }

        public string RosterScopeName { get; }
        public string ParentScopeTypeName { get; }

        public string MemberName => CodeGenerator.PrivateFieldsPrefix + VariableName;
        public string IdName => CodeGenerator.GetQuestionIdName(VariableName);
        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
        public string ExpressionMethodName => CodeGenerator.VariablePrefix + VariableName;
    }
}