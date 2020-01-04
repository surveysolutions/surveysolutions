using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
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
            Guid userId = Id.g1;
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

        [Test]
        public void when_get_record_for_range_of_dates_should_return_correct_data()
        {
            Guid userId = Id.g1;
            DateTime startDate = new DateTime(2018, 7, 14);
            DateTime endDate = new DateTime(2019, 7, 14);
            
            AuditLogRecord[] records = new[]
            {
                new AuditLogRecord()
                {
                    Type = AuditLogEntityType.SynchronizationCanceled,
                    Time = new DateTime(2019, 1, 14),
                }, 
            };

            var auditLogFactory = Mock.Of<IAuditLogFactory>(f => f.GetRecords(userId, startDate, endDate) == records);

            var auditLogService = Create.Service.AuditLogService(auditLogFactory);

            var queryResult = auditLogService.GetRecords(userId, startDate, endDate).ToList();

            Assert.That(queryResult.Count, Is.EqualTo(records.Count()));
            Assert.That(queryResult.Single().Type, Is.EqualTo(records.Single().Type));
            Assert.That(queryResult.Single().Message, Is.Not.Empty);
            Assert.That(queryResult.Single().Time, Is.EqualTo(records.Single().Time));
            Assert.That(queryResult.Single().Description, Is.Null);
        }
    }
}
