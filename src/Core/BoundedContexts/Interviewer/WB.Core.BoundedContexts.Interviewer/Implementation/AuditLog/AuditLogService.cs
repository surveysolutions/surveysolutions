using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IPlainStorage<AutoincrementKeyValue, int> auditLogStorage;
        private readonly IPlainStorage<AuditLogSettingsView> auditLogSettingsStorage;
        private readonly IUserIdentity userIdentity;
        private readonly IDeviceInformationService deviceInformationService;
        private readonly IJsonAllTypesSerializer serializer;

        private const string AuditLogSettingsKey = "settings";

        public AuditLogService(IPlainStorage<AutoincrementKeyValue, int> auditLogStorage,
            IPlainStorage<AuditLogSettingsView> auditLogSettingsStorage,
            IUserIdentity userIdentity,
            IDeviceInformationService deviceInformationService,
            IJsonAllTypesSerializer serializer)
        {
            this.auditLogStorage = auditLogStorage;
            this.auditLogSettingsStorage = auditLogSettingsStorage;
            this.userIdentity = userIdentity;
            this.deviceInformationService = deviceInformationService;
            this.serializer = serializer;
        }

        public class AutoincrementKeyValue : IPlainStorageEntity<int>
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Json { get; set; }
        }

        public void Write(IAuditLogEntity entity)
        {
            var userId = userIdentity.UserId;
            var versionCode = deviceInformationService.TryGetApplicationVersionCode();

            var auditLogEntityView = new AuditLogEntityView()
            {
                ResponsibleId = userId,
                VersionCode = versionCode,
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

        public void UpdateSyncIndex(int id)
        {
            var settingsView = auditLogSettingsStorage.GetById(AuditLogSettingsKey);
            settingsView.LastSyncedEntityId = id;
            auditLogSettingsStorage.Store(settingsView);
        }

        public IEnumerable<AuditLogEntityView> GetAuditLogEntitiesForSync()
        {
            var settingsView = auditLogSettingsStorage.GetById(AuditLogSettingsKey);
            var lastSyncedEntityId = settingsView.LastSyncedEntityId;
            return auditLogStorage.Where(kv => kv.Id > lastSyncedEntityId)
                .Select(kv => serializer.Deserialize<AuditLogEntityView>(kv.Json))
                .ToList();
        }
    }
}
