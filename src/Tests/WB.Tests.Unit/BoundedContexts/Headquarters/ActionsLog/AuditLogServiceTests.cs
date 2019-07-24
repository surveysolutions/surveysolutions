using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ActionsLog
{
    [TestOf(typeof(AuditLogService))]
    public class AuditLogServiceTests
    {
        [Test]
        public void when_get_record_for_7_days_should_be_returned()
        {
            Guid userId = Guid.NewGuid();
            DateTime startDate = new DateTime(2019, 7, 14);
            
            AuditLogResult result = new AuditLogResult()
            {
                NextBatchRecordDate = startDate.AddDays(-7),
                Records = new[]
                {
                    new AuditLogRecord() { Type = AuditLogEntityType.SynchronizationCanceled  }, 
                }
            };

            var auditLogFactory = Mock.Of<IAuditLogFactory>(f => f.GetLastExisted7DaysRecords(userId, startDate) == result);

            var auditLogService = Create.Service.AuditLogService(auditLogFactory);

            var queryResult = auditLogService.GetLastExisted7DaysRecords(userId, startDate);

            Assert.That(queryResult.NextBatchRecordDate, Is.EqualTo(result.NextBatchRecordDate));
            Assert.That(queryResult.Items.Length, Is.EqualTo(result.Records.Count()));
            Assert.That(queryResult.Items.Single().Type, Is.EqualTo(result.Records.Single().Type));
            Assert.That(queryResult.Items.Single().Message, Is.Not.Empty);
        }
    }
}
