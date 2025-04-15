using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [TestOf(typeof(InterviewSummaryDenormalizer))]
    public class InterviewSummaryDenormalizerTests
    {
        private InterviewSummaryDenormalizer denormalizer;
        private TenantDbContext dbContext;

        [SetUp]
        public void Setup()
        {
            dbContext = Create.TenantDbContext();
            denormalizer = new InterviewSummaryDenormalizer(dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Dispose();
            dbContext = null;
        }

        [Test]
        public async Task when_handle_answer_commented_event_should_update_interview_reference()
        {
            // Arrange
            var questionnaireId = "qu1";
            var reference = Create.InterviewReference(questionnaireId: questionnaireId);
            this.dbContext.Add(reference);
            await this.dbContext.SaveChangesAsync();
            
            var eventTimeStamp = DateTime.UtcNow;
            var @event = new PublishedEvent<AnswerCommented>(
                 globalSequence:2,
                 sequence: 2,
                 eventSourceId: reference.InterviewId,
                 eventTimeStamp: eventTimeStamp,
                 @event: new AnswerCommented()
            );
            
            // Act
            denormalizer.Handle(@event);
            await denormalizer.SaveStateAsync(CancellationToken.None);

            // Assert
            Assert.That(eventTimeStamp, Is.EqualTo( reference.UpdateDateUtc));
        }
    }
}
