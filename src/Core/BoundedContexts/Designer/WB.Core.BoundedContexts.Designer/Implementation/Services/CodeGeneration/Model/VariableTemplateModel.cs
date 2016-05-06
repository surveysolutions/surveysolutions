using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class VariableTemplateModel
    {
        public Guid Id { get; set; }
        public string VariableName { get; set; }
        public string Expression { get; set; }
        public string TypeName { get; set; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }

        public string MemberName => CodeGenerator.PrivateFieldsPrefix + VariableName;
        public string IdName => CodeGenerator.GetQuestionIdName(VariableName);
        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
    }
}