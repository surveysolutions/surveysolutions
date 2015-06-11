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
    internal class InterviewPreconditionsService : IInterviewPreconditionsService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings;

        public InterviewPreconditionsService(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, 
            InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.interviewPreconditionsServiceSettings = interviewPreconditionsServiceSettings;
        }

        public int? GetMaxAllowedInterviewsCount()
        {
            return interviewPreconditionsServiceSettings.InterviewLimitCount;
        }

        public int? GetInterviewsCountAllowedToCreateUntilLimitReached()
        {
            if (!interviewPreconditionsServiceSettings.InterviewLimitCount.HasValue)
                return null;

            return interviewPreconditionsServiceSettings.InterviewLimitCount -
                   interviewSummaryStorage.Query(_ => _.Select(i => i.InterviewId).Count());
        }
    }

    internal class InterviewPreconditionsServiceSettings
    {
        public InterviewPreconditionsServiceSettings(int? interviewLimitCount)
        {
            InterviewLimitCount = interviewLimitCount;
        }

        public int? InterviewLimitCount { get; private set; }
    }
}
