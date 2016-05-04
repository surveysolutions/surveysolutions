using System;
using WB.Core.SharedKernels.DataCollection.V4.CustomFunctions;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewProperties : IInterviewProperties
    {
        public Guid Id { get; private set; }

        private double randomDouble { get; set; }

        
        public InterviewProperties(Guid id)
        {
            this.Id = id;
            randomDouble = Id.GetRandomDouble();
        }

        public IInterviewProperties Clone()
        {
            return new InterviewProperties(this.Id);
        }

        public double IRnd()
        {
            return randomDouble;
        }
    }
}
