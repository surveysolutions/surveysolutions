using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class EnumeratorAuditLogService : IAuditLogService
    {
        private readonly IPlainStorage<AuditLogRecordView, int?> auditLogStorage;
        private readonly IPlainStorage<AuditLogSettingsView> auditLogSettingsStorage;
        private readonly ISerializer serializer;
        private readonly ILogger logger;
        private readonly IPrincipal principal;
        private readonly IServiceProvider serviceProvider;

        private const string AuditLogSettingsKey = "settings";

        public EnumeratorAuditLogService(IPlainStorage<AuditLogRecordView, int?> auditLogStorage,
            IPlainStorage<AuditLogSettingsView> auditLogSettingsStorage,
            ISerializer serializer,
            ILogger logger,
            IPrincipal principal,
            IServiceProvider serviceProvider)
        {
            this.auditLogStorage = auditLogStorage;
            this.auditLogSettingsStorage = auditLogSettingsStorage;
            this.serializer = serializer;
            this.logger = logger;
            this.principal = principal;
            this.serviceProvider = serviceProvider;
        }

        [Workspaces]
        public class AuditLogRecordView : IPlainStorageEntity<int?>
        {
            [PrimaryKey, Unique, AutoIncrement]
            public int? Id { get; set; }
            public string Json { get; set; }
        }
        
        public void Write(IAuditLogEntity entity)
        {
            var workspaceAccessor = (IWorkspaceAccessor) serviceProvider.GetService(typeof(IWorkspaceAccessor));
            var currentWorkspace = workspaceAccessor.GetCurrentWorkspaceName();

            WriteRecord(entity, currentWorkspace);
        }
        
        public void WriteApplicationLevelRecord(IAuditLogEntity entity)
        {
            WriteRecord(entity, null);
        }

        private void WriteRecord(IAuditLogEntity entity, string workspace)
        {
            try
            {
                var userIdentity = this.principal.CurrentUserIdentity;

                var auditLogEntityView = new AuditLogEntityView()
                {
                    ResponsibleId = userIdentity?.UserId,
                    ResponsibleName = userIdentity?.Name,
                    Time = DateTime.Now,
                    TimeUtc = DateTime.UtcNow,
                    Type = entity.Type,
                    Payload = entity,
                    Workspace = workspace,
                };

                WriteAuditLogRecord(auditLogEntityView);

                this.logger.Info(
                    $"{entity.GetType().Name.Replace("AuditLogEntity", "")} {this.serializer.SerializeWithoutTypes(entity)}");
            }
            catch (Exception e)
            {
                logger.Error("Error write to interviewer audit log", e);
            }
        }

        public void WriteAuditLogRecord(AuditLogEntityView auditLogEntityView)
        {
            var json = serializer.Serialize(auditLogEntityView);
            auditLogStorage.Store(new AuditLogRecordView { Json = json });
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
            foreach (var logItem in auditLogStorage.Where(kv => kv.Id > lastSyncedEntityId))
            {
                var entity = serializer.Deserialize<AuditLogEntityView>(logItem.Json);
                entity.Id = logItem.Id.Value;
                yield return entity;
            }
        }
    }
}
