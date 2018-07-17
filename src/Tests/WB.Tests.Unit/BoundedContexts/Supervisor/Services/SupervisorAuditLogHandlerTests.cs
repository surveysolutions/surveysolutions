using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorAuditLogHandler))]
    public class SupervisorAuditLogHandlerTests
    {
        [Test]
        public async Task should_handle_auditlog_uploads()
        {
            var fixture = Create.Other.AutoFixture();
            fixture.Register<IJsonAllTypesSerializer>(() => new JsonAllTypesSerializer());

            var auditlogService = fixture.Freeze<Mock<IAuditLogService>>();
            var subj = fixture.Create<SupervisorAuditLogHandler>();

            var payloads = new IAuditLogEntity[]
            {
                Create.Entity.AuditLogEntity.LogoutAuditLogEntity("int"),
                Create.Entity.AuditLogEntity.CloseInterviewAuditLogEntity()
            };

            await subj.UploadAuditLog(new UploadAuditLogEntityRequest
            {
                AuditLogEntity = new AuditLogEntitiesApiView
                {
                    Entities = new[]
                    {
                        Create.Entity.AuditLogEntity.AuditLogEntitiesApiView(
                            Create.Entity.AuditLogEntity.LogoutAuditLogEntity("int")),
                        Create.Entity.AuditLogEntity.AuditLogEntitiesApiView(
                            Create.Entity.AuditLogEntity.CloseInterviewAuditLogEntity())
                    }
                }
            });

            auditlogService.Verify(als => als.WriteAuditLogRecord(It.IsAny<AuditLogEntityView>()), Times.Exactly(2));
        }
    }
}