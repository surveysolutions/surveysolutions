using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionTemplateModel : ITemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        
        public string Conditions { set; get; }
        public string Validations { set; get; }

        public QuestionType QuestionType { set; get; }

        public string GeneratedIdName { set; get; }
        public string GeneratedTypeName { set; get; }
        public string GeneratedMemberName { set; get; }
        public string GeneratedStateName { set; get; }

        public string RosterScopeName { set; get; }

        public string GeneratedValidationsMethodName { set; get; }
        public string GeneratedConditionsMethodName { set; get; }

        public bool IsMultiOptionYesNoQuestion { get; set; }
    }
}