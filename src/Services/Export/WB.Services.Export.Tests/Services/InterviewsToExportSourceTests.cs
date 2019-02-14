using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;

namespace WB.Services.Export.Tests.Services
{
    [TestFixture]
    [TestOf(typeof(InterviewsToExportSource))]
    public class InterviewsToExportSourceTests
    {
        private ITenantContext tenantContext;
        private InterviewsToExportSource interviewsToExportSource;

        [SetUp]
        public void Setup()
        {
            var tenantContextMock = new Mock<ITenantContext>();
            tenantContextMock.Setup(x => x.Tenant)
                .Returns(Create.Tenant());

            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(databaseName: "TenantDbContext")
                .Options;
            var dbContext = new TenantDbContext(tenantContextMock.Object, 
                Mock.Of<IOptions<DbConnectionSettings>>(x => x.Value == new DbConnectionSettings()), 
                options);
            
            tenantContextMock.Setup(x => x.DbContext)
                .Returns(dbContext);

            this.tenantContext = tenantContextMock.Object;
            this.interviewsToExportSource = new InterviewsToExportSource(this.tenantContext);

        }

        [TearDown]
        public void TearDown()
        {
            this.tenantContext.DbContext.Dispose();
        }

        [Test]
        public void should_be_able_to_read_empty_db()
        {
            var interviews = this.interviewsToExportSource.GetInterviewsToExport(new QuestionnaireId("id"), null, null, null);

            Assert.That(interviews, Is.Empty);
        }

        [Test]
        public void should_be_able_to_read_interviews_by_status()
        {
            var questionnaireId = "qu1";
            var reference = Create.InterviewReference(questionnaireId: questionnaireId, status: InterviewStatus.Completed);
            this.tenantContext.DbContext.Add(reference);
            this.tenantContext.DbContext.SaveChanges();

            // Act
            var found = this.interviewsToExportSource.GetInterviewsToExport(new QuestionnaireId(questionnaireId),
                InterviewStatus.Completed, null, null);

            Assert.That(found, Has.Count.EqualTo(1));
        }

        [Test]
        public void should_be_able_read_interviews_by_date()
        {
            var questionnaireId = "qu1";
            var updateDateUtc = new DateTime(2010, 10, 5);
            var reference = Create.InterviewReference(questionnaireId: questionnaireId, updateDateUtc: updateDateUtc);
            this.tenantContext.DbContext.Add(reference);
            this.tenantContext.DbContext.SaveChanges();

            // Act
            var found = this.interviewsToExportSource.GetInterviewsToExport(new QuestionnaireId(questionnaireId),
                null, updateDateUtc.AddDays(-1), updateDateUtc.AddDays(1));

            Assert.That(found, Has.Count.EqualTo(1));
        }
    }
}
