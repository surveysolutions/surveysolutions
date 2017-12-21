using System;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class QuestionModel
    {
        public Guid Id { set; get; }

        public string Variable { set; get; }

        public string TypeName => this.IsValueType ? this.RawTypeName + "?" : RawTypeName;

        public string RawTypeName { get; set; }

        public bool IsValueType { get; set; }

        public RosterScope RosterScope { get; set; }

        public string GetGetAnswerMethodName()
        {
            return this.IsValueType ? nameof(IInterviewStateForExpressions.GetAnswerValueType) : nameof(IInterviewStateForExpressions.GetAnswer);
        }
    }
}