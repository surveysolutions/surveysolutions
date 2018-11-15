using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    internal class InterviewUniqueKeyGenerator : IInterviewUniqueKeyGenerator
    {
        private static int randomValueIndex = 0;

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IRandomValuesSource randomValuesSource;
        private const int maxInterviewKeyValue = 99999999;

        public InterviewUniqueKeyGenerator(IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            IRandomValuesSource randomValuesSource)
        {
            this.summaries = summaries;
            this.randomValuesSource = randomValuesSource;
        }

        public InterviewKey Get()
        {
            var potentialRandomKeys = this.GetRandomSequence();

            string[] stringKeys = potentialRandomKeys.Select(x => x.ToString()).ToArray();
            List<string> usedIds = this.summaries.Query(_ => _.Where(x => stringKeys.Contains(x.Key)).Select(x => x.Key).ToList());

            int nextIndexToUse = Interlocked.Increment(ref randomValueIndex);

            List<string> idsStrings = stringKeys.Where(x => !usedIds.Contains(x)).ToList();
            if (idsStrings.Count > 0)
            {
                string uniqueIdString = idsStrings[nextIndexToUse % idsStrings.Count];
                if (uniqueIdString != null) return InterviewKey.Parse(uniqueIdString);
            }

            return this.Get();
        }

        private List<InterviewKey> GetRandomSequence()
            => Enumerable.Range(1, 30).Select(_ => new InterviewKey(this.randomValuesSource.Next(maxInterviewKeyValue))).ToList();
    }
}
