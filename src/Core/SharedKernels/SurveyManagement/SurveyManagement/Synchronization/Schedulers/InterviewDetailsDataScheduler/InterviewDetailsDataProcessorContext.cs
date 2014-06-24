using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsDataProcessorContext : SynchorizationContext
    {
        /// <remarks>For tests only.</remarks>
        internal InterviewDetailsDataProcessorContext()
            : base(null) {}

        public InterviewDetailsDataProcessorContext(IPlainStorageAccessor<SynchronizationStatus> statusStorage)
            : base(statusStorage) {}

        protected override string StorageDocumentId
        {
            get { return "InterviewDetailsPushStatus"; }
        }
    }
}