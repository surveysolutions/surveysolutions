using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class LinkedQuestionFilterExpressionModel
    {
        public LinkedQuestionFilterExpressionModel(string filterExpression, string filterForLinkedQuestionMethodName, string rosterVariableName, Guid rosterId, Guid linkedQuestionId)
        {
            this.FilterExpression = filterExpression;
            this.FilterForLinkedQuestionMethodName = filterForLinkedQuestionMethodName;
            this.RosterVariableName = rosterVariableName;
            this.RosterId = rosterId;
            this.LinkedQuestionId = linkedQuestionId;
        }

        public string FilterExpression { get; private set; }
        public string FilterForLinkedQuestionMethodName { get; private set; }
        public string RosterVariableName { get; private set; }
        public Guid RosterId { get; private set; }
        public Guid LinkedQuestionId { get; private set; }
    }
}