using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionTemplateModel : ITemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        
        public string Condition { set; get; }
        public string Validation { set; get; }

        public string TypeName { set; get; }

        public string ConditionMethodName => "IsEnabled_" + this.VariableName;

        public string ValidationMethodName => "IsValid_" + VariableName;
        public string MemberName => CodeGenerator.PrivateFieldsPrefix + VariableName;
        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + "_state";
        public string IdName => CodeGenerator.PrivateFieldsPrefix + VariableName + "_id";

        public bool IsMultiOptionYesNoQuestion { get; set; }
        public List<string> AllMultioptionYesNoCodes { get; set; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }
    }
}