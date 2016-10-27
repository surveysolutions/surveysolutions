using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading
{
    public class PreloadedLevelDto
    {
        public PreloadedLevelDto(decimal[] rosterVector, Dictionary<Guid, AbstractAnswer> answers)
        {
            this.RosterVector = rosterVector;
            this.Answers = answers;
        }

        public decimal[] RosterVector { get; private set; }
        public Dictionary<Guid, AbstractAnswer> Answers { get; private set; }
    }
}
