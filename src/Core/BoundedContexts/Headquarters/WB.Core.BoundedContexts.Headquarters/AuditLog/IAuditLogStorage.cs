using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.AuditLog
{
    public interface IAuditLogStorage
    {
        void Store(AuditLogRecord record);
    }

    class AuditLogStorage : IAuditLogStorage
    {
        private readonly IPlainStorageAccessor<AuditLogRecord> storageAccessor;

        public AuditLogStorage(IPlainStorageAccessor<AuditLogRecord> storageAccessor)
        {
            this.storageAccessor = storageAccessor;
        }

        public void Store(AuditLogRecord record)
        {
            storageAccessor.Store(record, record.Id);
        }
    }
}
