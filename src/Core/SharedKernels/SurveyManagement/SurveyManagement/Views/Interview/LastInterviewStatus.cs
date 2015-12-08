using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class LastInterviewStatus : IReadSideRepositoryEntity
    {
        protected LastInterviewStatus() {}

        public LastInterviewStatus(string entryId, InterviewStatus status)
        {
            this.EntryId = entryId;
            this.Status = status;
        }

        public virtual string EntryId { get; set; }

        public virtual InterviewStatus Status { get; set; }
    }
}