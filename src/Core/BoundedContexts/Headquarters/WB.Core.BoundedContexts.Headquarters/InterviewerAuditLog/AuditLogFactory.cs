using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    class AuditLogFactory : IAuditLogFactory
    {
        private readonly IPlainStorageAccessor<AuditLogRecord> storageAccessor;

        public AuditLogFactory(IPlainStorageAccessor<AuditLogRecord> storageAccessor)
        {
            this.storageAccessor = storageAccessor;
        }

        public IEnumerable<AuditLogRecord> GetRecords(Guid responsibleId, DateTime startDateTime, DateTime endDateTime)
        {
            return GetRecordsImpl(responsibleId, startDate: startDateTime, endDate: endDateTime);
        }

        public IEnumerable<AuditLogRecord> GetRecordsFor7Days(Guid responsibleId)
        {
            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
            return GetRecordsImpl(responsibleId, startDate: sevenDaysAgo, endDate: null);
        }

        public IEnumerable<AuditLogRecord> GetRecords(Guid responsibleId)
        {
            return GetRecordsImpl(responsibleId, startDate: null, endDate: null);
        }

        private IEnumerable<AuditLogRecord> GetRecordsImpl(Guid responsibleId, DateTime? startDate, DateTime? endDate)
        {
            return storageAccessor.Query(_ =>
            {
                var records = _.Where(r => r.ResponsibleId == responsibleId);

                if (startDate.HasValue)
                    records = records.Where(r => r.TimeUtc >= startDate);

                if (endDate.HasValue)
                    records = records.Where(r => r.TimeUtc < endDate);

                return records.OrderByDescending(r => r.RecordId);
            }).ToList();
        }
    }
}
