using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class LimitedInterviewPreconditionsService : IInterviewPreconditionsService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly LimitedInterviewSettings limitedInterviewSettings;

        public LimitedInterviewPreconditionsService(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, LimitedInterviewSettings limitedInterviewSettings)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.limitedInterviewSettings = limitedInterviewSettings;
        }

        public long? MaxNumberOfInterviews
        {
            get { return limitedInterviewSettings.InterviewLimitCount; }
        }

        public long? NumberofInterviewsAllowedToCreate
        {
            get { return limitedInterviewSettings.InterviewLimitCount - interviewSummaryStorage.Query(_ => _.Select(i => i.InterviewId).Count()); }
        }
    }

    internal class LimitedInterviewSettings
    {
        public LimitedInterviewSettings(long interviewLimitCount)
        {
            InterviewLimitCount = interviewLimitCount;
        }

        public long InterviewLimitCount { get; private set; }
    }
}
