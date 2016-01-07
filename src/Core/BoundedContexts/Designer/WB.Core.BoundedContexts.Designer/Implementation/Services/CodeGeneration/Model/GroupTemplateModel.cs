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

        public string StateName => "@__" + VariableName + "_state";
        public string IdName => "@__" + VariableName + "_id";
        public string ConditionMethodName => "IsEnabled_" + VariableName;
    }
}