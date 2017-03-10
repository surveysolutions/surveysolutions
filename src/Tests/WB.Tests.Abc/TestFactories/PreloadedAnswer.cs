using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Abc.TestFactories
{
    internal class PreloadedAnswer
    {
        public Guid Id { get; private set; }
        public AbstractAnswer Answer { get; private set; }

        public PreloadedAnswer(Guid id, AbstractAnswer answer = null)
        {
            this.Id = id;
            this.Answer = answer;
        }
    }
}