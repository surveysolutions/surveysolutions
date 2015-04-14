using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class InterviewModel : IInterview
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid Id { get; set; }

        public Dictionary<string, AbstractInterviewQuestion> Answers { get; set; }

        // TODO delete after implimentation, this is only dummy method
        public T GetAnswerOnQuestion<T>(Guid questionId)
        {
            return default(T);
        }
    }
}