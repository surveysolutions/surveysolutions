using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.BaseStructures
{
    public interface IExpressionProcessor
    {
        void AddRoster(Guid rosterId, decimal[] rosterVector);
        void RemoveRoster(Guid rosterId, decimal[] rosterVector);
        void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, int answer);
        void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer);
        void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer);
        void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer);
        void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer);
        void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer);
        void ProcessExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }
}