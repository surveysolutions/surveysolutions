namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenReadSideRepositoryWriterSettings
    {
        public RavenReadSideRepositoryWriterSettings(int bulkInsertBatchSize)
        {
            this.BulkInsertBatchSize = bulkInsertBatchSize;
        }

        public int BulkInsertBatchSize { get; private set; }
    }
}
