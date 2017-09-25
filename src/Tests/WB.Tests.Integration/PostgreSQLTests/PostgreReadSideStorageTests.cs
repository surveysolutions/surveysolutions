using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.Infrastructure.Transactions;
using WB.Tests.Abc;

namespace WB.Tests.Integration.PostgreSQLTests
{
    [TestFixture]
    internal class PostgreReadSideStorageTests : NpgsqlStorageContext
    {
        [Test]
        public void When_getting_records_what_starts_with_specified_string()
        {
            List<InterviewDataExportRecord> exportRecords = new List<InterviewDataExportRecord>
            {
                Create.Entity.InterviewDataExportRecord(id: "i1$level1", interviewId: Guid.NewGuid()),
                Create.Entity.InterviewDataExportRecord(id: "i2$roster1", interviewId: Guid.NewGuid()),
                Create.Entity.InterviewDataExportRecord(id: "i1$roster2", interviewId: Guid.NewGuid()),
                Create.Entity.InterviewDataExportRecord(id: "i2$roster2", interviewId: Guid.NewGuid())
            };

            var storage = CreateInterviewExportRepository();
            ExecuteInCommandTransaction(() => exportRecords.ForEach(x => storage.Store(x, x.Id)));

            var ids = transactionManager.ExecuteInQueryTransaction(() => storage.GetIdsStartWith("i1").ToArray());
        
            CollectionAssert.AreEqual(new [] { "i1$level1", "i1$roster2" }, ids);
        }
    }
}

