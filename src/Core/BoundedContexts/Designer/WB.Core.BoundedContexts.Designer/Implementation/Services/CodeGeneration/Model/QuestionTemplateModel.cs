using System;
using System.Collections.Generic;

using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionTemplateModel : ITemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        
        public string Condition { set; get; }
        public string Validation { set; get; }

        public QuestionType QuestionType { set; get; }

        public string GeneratedIdName { set; get; }
        public string GeneratedTypeName { set; get; }
        public string GeneratedMemberName { set; get; }
        public string GeneratedStateName { set; get; }

        public string GeneratedValidationsMethodName { set; get; }
        public string GeneratedConditionsMethodName { set; get; }

        public bool IsMultiOptionYesNoQuestion { get; set; }
        public List<string> AllMultioptionYesNoCodes { get; set; }

        public string RosterScopeName { set; get; }
        public string ParentScopeTypeName { get; set; }
    }
}