using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading
{
    public class PreloadedLevelDto
    {
        public PreloadedLevelDto(decimal[] rosterVector, Dictionary<Guid, object> answers)
        {
            this.RosterVector = rosterVector;
            this.Answers = answers;
        }

        public decimal[] RosterVector { get; private set; }
        public Dictionary<Guid, object> Answers { get; private set; }
    }
}
