using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class DeletedQuestionnaireEventFilterTests
    {
        private ITenantContext tenantContext;
        private TenantDbContext dbContext;

        [SetUp]
        public void Setup()
        {
            var tenantContextMock = new Mock<ITenantContext>();
            tenantContextMock.Setup(x => x.Tenant)
                .Returns(Create.Tenant());

            dbContext = Create.TenantDbContext();

            this.tenantContext = tenantContextMock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Dispose();
            dbContext = null;
        }

        [Test]
        public async Task should_be_able_to_handle_two_interview_created_events()
        {
            var interviewId = Id.g1;

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                id: Id.gA,
                children: Create.TextQuestion(Id.gB));
            questionnaire.Id = $"{Id.gA:N}$1";

            var eventFeed = new List<Event>
            {
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new InterviewCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                },
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new InterviewOnClientCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                }

            };

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage);

            // Act
            var result = await filter.FilterAsync(eventFeed);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));

            var storedReference = dbContext.InterviewReferences.Find(interviewId);
            Assert.That(storedReference, Is.Not.Null, "Should store questionnaire id <-> interview id relation");
        }

        private DeletedQuestionnaireEventFilter CreateFilter(ITenantContext tenantContext = null,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new DeletedQuestionnaireEventFilter
            (
                tenantContext ?? this.tenantContext,
                dbContext,
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>()
            );
        }
    }
}
