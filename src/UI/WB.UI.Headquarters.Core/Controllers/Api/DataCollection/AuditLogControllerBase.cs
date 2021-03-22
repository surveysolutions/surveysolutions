using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class AuditLogControllerBase : ControllerBase
    {
        private readonly IPlainStorageAccessor<AuditLogRecord> auditLogStorage;
        private readonly IPlainStorageAccessor<GlobalAuditLogRecord> globalAuditLogStorage;
        private readonly IInScopeExecutor scopeExecutor;
        private readonly IWorkspacesCache workspacesCache;
        private readonly ILogger<AuditLogControllerBase> logger;

        protected AuditLogControllerBase(IPlainStorageAccessor<AuditLogRecord> auditLogStorage,
            IPlainStorageAccessor<GlobalAuditLogRecord> globalAuditLogStorage,
            IInScopeExecutor scopeExecutor,
            IWorkspacesCache workspacesCache,
            ILogger<AuditLogControllerBase> logger)
        {
            this.auditLogStorage = auditLogStorage;
            this.globalAuditLogStorage = globalAuditLogStorage;
            this.scopeExecutor = scopeExecutor;
            this.workspacesCache = workspacesCache;
            this.logger = logger;
        }

        public virtual IActionResult Post(AuditLogEntitiesApiView entities)
        {
            if (entities == null)
                return this.BadRequest("Server cannot accept empty package content.");

            if (entities.IsWorkspaceSupported)
            {
                var workspaces = workspacesCache.AllWorkspaces();
                foreach (var auditLogEntity in entities.Entities)
                {
                    var workspace = auditLogEntity.Workspace;
                    if (string.IsNullOrEmpty(workspace))
                    {
                        var auditLogRecord = new GlobalAuditLogRecord()
                        {
                            RecordId = auditLogEntity.Id,
                            ResponsibleId = auditLogEntity.ResponsibleId,
                            ResponsibleName = auditLogEntity.ResponsibleName,
                            Time = auditLogEntity.Time.DateTime,
                            TimeUtc = auditLogEntity.TimeUtc.DateTime,
                            Type = auditLogEntity.Type,
                        };
                        auditLogRecord.SetJsonPayload(auditLogEntity.Payload);
                        globalAuditLogStorage.Store(auditLogRecord, auditLogRecord.Id);
                    }
                    else
                    {
                        if (workspaces.Any(w => w.Name == workspace))
                        {
                            scopeExecutor.Execute(sl =>
                            {
                                var auditLogRecord = new AuditLogRecord()
                                {
                                    RecordId = auditLogEntity.Id,
                                    ResponsibleId = auditLogEntity.ResponsibleId,
                                    ResponsibleName = auditLogEntity.ResponsibleName,
                                    Time = auditLogEntity.Time.DateTime,
                                    TimeUtc = auditLogEntity.TimeUtc.DateTime,
                                    Type = auditLogEntity.Type,
                                };
                                auditLogRecord.SetJsonPayload(auditLogEntity.Payload);
                                auditLogStorage.Store(auditLogRecord, auditLogRecord.Id);
                            }, workspace);
                        }
                        else
                        {
                            logger.LogWarning("Audit log message ignored, because workspace {workspace} not exists. Payload {payload}", workspace, auditLogEntity.Payload);
                        }
                    }
                }
            }
            else
            {
                // old tablets
                foreach (var auditLogEntity in entities.Entities)
                {
                    var auditLogRecord = new AuditLogRecord()
                    {
                        RecordId = auditLogEntity.Id,
                        ResponsibleId = auditLogEntity.ResponsibleId,
                        ResponsibleName = auditLogEntity.ResponsibleName,
                        Time = auditLogEntity.Time.DateTime,
                        TimeUtc = auditLogEntity.TimeUtc.DateTime,
                        Type = auditLogEntity.Type,
                    };
                    auditLogRecord.SetJsonPayload(auditLogEntity.Payload);
                    auditLogStorage.Store(auditLogRecord, auditLogRecord.Id);
                }
            }


            return this.Ok();
        }
    }
}
