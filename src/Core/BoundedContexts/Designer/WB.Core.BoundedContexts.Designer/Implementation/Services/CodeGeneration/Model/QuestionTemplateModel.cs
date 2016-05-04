using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionTemplateModel : ITemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Condition { set; get; }

        public List<ValidationExpressionModel> ValidationExpressions { get; set; }

        public string TypeName { set; get; }
        public string ConditionMethodName => CodeGenerator.EnablementPrefix + this.VariableName;

        public string MemberName => CodeGenerator.PrivateFieldsPrefix + VariableName;
        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.GetQuestionIdName(VariableName);

        public bool IsMultiOptionYesNoQuestion { get; set; }
        public List<string> AllMultioptionYesNoCodes { get; set; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }

    }
}