using System;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.Services
{
    public class InterviewerInterviewUniqueKeyGenerator : IInterviewUniqueKeyGenerator
    {
        private readonly IPlainStorage<InterviewView> localInterviews;
        private static readonly Random Random = new Random();
        private const int maxInterviewKeyValue = 99999999;

        public InterviewerInterviewUniqueKeyGenerator(IPlainStorage<InterviewView> localInterviews)
        {
            this.localInterviews = localInterviews ?? throw new ArgumentNullException(nameof(localInterviews));
        }

        public InterviewKey Get()
        {
            while (true) // otherwise access to modified closure warning appears
            {
                int val = Random.Next(maxInterviewKeyValue);
                var key = new InterviewKey(val);

                string interviewStringKey = key.ToString();
                if (localInterviews.Count(x => x.InterviewKey == interviewStringKey) == 0)
                {
                    return key;
                }
            }
        }
    }
}
