using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog
{
    class AuditLogFactory : IAuditLogFactory
    {
        private readonly IPlainStorageAccessor<AuditLogRecord> storageAccessor;
        private readonly IPlainStorageAccessor<GlobalAuditLogRecord> globalStorageAccessor;

        public AuditLogFactory(IPlainStorageAccessor<AuditLogRecord> storageAccessor,
            IPlainStorageAccessor<GlobalAuditLogRecord> globalStorageAccessor)
        {
            this.storageAccessor = storageAccessor;
            this.globalStorageAccessor = globalStorageAccessor;
        }

        public IEnumerable<AuditLogRecord> GetRecords(Guid responsibleId, DateTime startDateTime, DateTime endDateTime)
        {
            return GetRecordsImpl(responsibleId, startDate: startDateTime, endDate: endDateTime);
        }

        public AuditLogResult GetLastExisted7DaysRecords(Guid responsibleId, DateTime dateTime)
        {
            var last8Days = storageAccessor.Query(_ => _.Where(r => r.ResponsibleId == responsibleId && r.TimeUtc.Date <= dateTime)
                .Select(r => r.Time.Date)
                .GroupBy(r => r.Date)
                .Select(r => r.Key)
                .OrderByDescending(r => r)
                .Take(8))
                .ToList();

            var last7Days = last8Days.Take(7).ToList();

            var records = storageAccessor.Query(_ =>
                _.Where(r => r.ResponsibleId == responsibleId && last7Days.Contains(r.TimeUtc.Date))
                    .OrderByDescending(r => r.Id)
            ).ToList();

            return new AuditLogResult()
            {
                Records = records,
                NextBatchRecordDate = last8Days.Count == 8 ? last8Days[7] : (DateTime?)null
            };
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
            var workspaceRecords = storageAccessor.Query(_ =>
            {
                var records = _.Where(r => r.ResponsibleId == responsibleId);

                if (startDate.HasValue)
                    records = records.Where(r => r.TimeUtc >= startDate);

                if (endDate.HasValue)
                    records = records.Where(r => r.TimeUtc < endDate);

                return records.OrderByDescending(r => r.Id);
            }).ToList();

            if (workspaceRecords.Count == 0)
                return workspaceRecords;

            var globalRecords = globalStorageAccessor.Query(_ =>
            {
                var records = _.Where(r => r.ResponsibleId == responsibleId);

                if (startDate.HasValue)
                    records = records.Where(r => r.TimeUtc >= startDate);

                if (endDate.HasValue)
                    records = records.Where(r => r.TimeUtc < endDate);

                return records.OrderByDescending(r => r.Id);
            }).ToList();
            
            return workspaceRecords.Concat(globalRecords).OrderByDescending(r => r.TimeUtc);
        }
    }
}
