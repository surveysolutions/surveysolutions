using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Polly;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    internal class InterviewUniqueKeyGenerator : IInterviewUniqueKeyGenerator
    {
        private static int randomValueIndex = 0;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IPlainKeyValueStorage<NaturalKeySettings> naturalKeySettings;
        private readonly IRandomValuesSource randomValuesSource;
        private readonly ILogger<InterviewUniqueKeyGenerator> logger;
        public static int maxInterviewKeyValue = 0;

        public InterviewUniqueKeyGenerator(IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            IPlainKeyValueStorage<NaturalKeySettings> naturalKeySettings,
            IRandomValuesSource randomValuesSource,
            ILogger<InterviewUniqueKeyGenerator> logger)
        {
            this.summaries = summaries;
            this.naturalKeySettings = naturalKeySettings;
            this.randomValuesSource = randomValuesSource;
            this.logger = logger;
        }

        public InterviewKey Get()
        {
            if (maxInterviewKeyValue == 0)
            {
                NaturalKeySettings storedMaxValue = naturalKeySettings.GetById(AppSetting.NatualKeySettings);
                maxInterviewKeyValue = storedMaxValue?.MaxValue ?? 99_99_99_99;
            }

            var result = Policy.Handle<InterviewUniqueKeyGeneratorException>()
                .Retry(5, (ctx, retryCount) =>
                {
                    logger.LogWarning("Failed to generate human id. Retry count {retry}", retryCount);
                    if (retryCount > 3 && maxInterviewKeyValue != int.MaxValue)
                    {
                        NaturalKeySettings storedMaxValue = naturalKeySettings.GetById(AppSetting.NatualKeySettings) ?? new NaturalKeySettings();
                        storedMaxValue.MaxValue = int.MaxValue;

                        naturalKeySettings.Store(storedMaxValue, AppSetting.NatualKeySettings);
                        maxInterviewKeyValue = storedMaxValue.MaxValue;
                    }
                })
                .Execute(GetKey);

            return result;
        }

        private InterviewKey GetKey()
        {
            var potentialRandomKeys = this.GetRandomSequence();

            string[] stringKeys = potentialRandomKeys.Select(x => x.ToString()).ToArray();
            List<string> usedIds = this.summaries.Query(_ => _.Where(x => stringKeys.Contains(x.Key))
                .Select(x => x.Key)
                .ToList());

            int nextIndexToUse = Interlocked.Increment(ref randomValueIndex);

            List<string> idsStrings = stringKeys.Where(x => !usedIds.Contains(x)).ToList();
            if (idsStrings.Count > 0)
            {
                string uniqueIdString = idsStrings[nextIndexToUse % idsStrings.Count];
                if (uniqueIdString != null) return InterviewKey.Parse(uniqueIdString);
            }

            throw new InterviewUniqueKeyGeneratorException("Failed to generate new interview key");
        }

        private List<InterviewKey> GetRandomSequence()
            => Enumerable.Range(1, 30).Select(_ => new InterviewKey(this.randomValuesSource.Next(maxInterviewKeyValue))).ToList();
    }
}
