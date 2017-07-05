using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading
{
    public class PreloadedDataDto
    {
        public PreloadedDataDto(PreloadedLevelDto[] data)
        {
            this.Data = data;
        }
        public PreloadedLevelDto[] Data { get; private set; }

        public List<InterviewAnswer> Answers => Data.SelectMany(x => x.Answers.Select(a => new InterviewAnswer
        {
            Identity = new Identity(a.Key, x.RosterVector),
            Answer = a.Value
        })).ToList();
    }
}
