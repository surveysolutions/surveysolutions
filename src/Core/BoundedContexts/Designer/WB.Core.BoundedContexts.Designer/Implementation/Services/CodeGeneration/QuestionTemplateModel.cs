using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionTemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }

        public string Conditions { set; get; }
        public string Validations { set; get; }

        public QuestionType QuestionType { set; get; }

        public string GeneratedIdName { set; get; }
        public string GeneratedQuestionTypeName { set; get; }
        public string GeneratedQuestionMemberName { set; get; }
        public string GeneratedQuestionStateName { set; get; }
    }
}