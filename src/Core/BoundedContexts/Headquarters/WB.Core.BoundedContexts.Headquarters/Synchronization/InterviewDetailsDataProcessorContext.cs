using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization
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