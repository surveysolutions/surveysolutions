using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class LinkedQuestionFilterExpressionModel
    {
        public LinkedQuestionFilterExpressionModel(string filterExpression, string filterForLinkedQuestionMethodName, string linkedQuestionVariableName, 
            Guid rosterId, Guid linkedQuestionId)
        {
            this.FilterExpression = filterExpression;
            this.FilterForLinkedQuestionMethodName = filterForLinkedQuestionMethodName;
            this.LinkedQuestionVariableName = linkedQuestionVariableName;
            this.RosterId = rosterId;
            this.LinkedQuestionId = linkedQuestionId;
        }

        public string FilterExpression { get; private set; }
        public string FilterForLinkedQuestionMethodName { get; private set; }
        
        public Guid RosterId { get; private set; }
        public Guid LinkedQuestionId { get; private set; }


        public string LinkedQuestionVariableName { get; private set; }
        public string ParentScopeTypeName { get; set; } = String.Empty;
    }
}
