using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IPlainStorage<AutoincrementKeyValue, int?> auditLogStorage;
        private readonly IPlainStorage<AuditLogSettingsView> auditLogSettingsStorage;
        private readonly IPlainStorage<InterviewerIdentity> userIdentity;
        private readonly ISerializer serializer;
        private readonly ILogger logger;

        private const string AuditLogSettingsKey = "settings";

        public AuditLogService(IPlainStorage<AutoincrementKeyValue, int?> auditLogStorage,
            IPlainStorage<AuditLogSettingsView> auditLogSettingsStorage,
            IPlainStorage<InterviewerIdentity> userIdentity,
            ISerializer serializer,
            ILogger logger)
        {
            this.auditLogStorage = auditLogStorage;
            this.auditLogSettingsStorage = auditLogSettingsStorage;
            this.userIdentity = userIdentity;
            this.serializer = serializer;
            this.logger = logger;
        }

        public class AutoincrementKeyValue : IPlainStorageEntity<int?>
        {
            [PrimaryKey, Unique, AutoIncrement]
            public int? Id { get; set; }
            public string Json { get; set; }
        }

        public void Write(IAuditLogEntity entity)
        {
            try
            {
                var interviewerIdentity = userIdentity.FirstOrDefault();

                var auditLogEntityView = new AuditLogEntityView()
                {
                    ResponsibleId = interviewerIdentity?.UserId,
                    ResponsibleName = interviewerIdentity?.Name,
                    Time = DateTime.Now,
                    TimeUtc = DateTime.UtcNow,
                    Type = entity.Type,
                    Payload = entity,
                };
                var json = serializer.Serialize(auditLogEntityView);
                auditLogStorage.Store(new AutoincrementKeyValue()
                {
                    Json = json
                });
            }
            catch (Exception e)
            {
                logger.Error("Error write to interviewer audit log", e);
            }
        }

        public void UpdateLastSyncIndex(int id)
        {
            var settingsView = new AuditLogSettingsView() { Id = AuditLogSettingsKey, LastSyncedEntityId = id };
            auditLogSettingsStorage.Store(settingsView);
        }

        public IEnumerable<AuditLogEntityView> GetAuditLogEntitiesForSync()
        {
            var settingsView = auditLogSettingsStorage.GetById(AuditLogSettingsKey);
            int lastSyncedEntityId = settingsView?.LastSyncedEntityId ?? -1;
            var list = auditLogStorage.Where(kv => kv.Id > lastSyncedEntityId).Select(kv =>
                new {
                    Id = kv.Id,
                    Entity = serializer.Deserialize<AuditLogEntityView>(kv.Json)
                }).ToList();
            // fix id
            list.ForEach(kv => kv.Entity.Id = kv.Id.Value);
            return list.Select(kv => kv.Entity).ToList();
        }
    }
}
