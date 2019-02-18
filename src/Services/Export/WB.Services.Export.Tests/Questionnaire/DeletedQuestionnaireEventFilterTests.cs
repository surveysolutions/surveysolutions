using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class DeletedQuestionnaireEventFilterTests
    {
        private ITenantContext tenantContext;

        [SetUp]
        public void Setup()
        {
            var tenantContextMock = new Mock<ITenantContext>();
            tenantContextMock.Setup(x => x.Tenant)
                .Returns(Create.Tenant());

            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var dbContext = new TenantDbContext(tenantContextMock.Object, 
                Mock.Of<IOptions<DbConnectionSettings>>(x => x.Value == new DbConnectionSettings()), 
                options);
            
            tenantContextMock.Setup(x => x.DbContext)
                .Returns(dbContext);

            this.tenantContext = tenantContextMock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            this.tenantContext.DbContext.Dispose();
            this.tenantContext = null;
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

            var storedReference = this.tenantContext.DbContext.InterviewReferences.Find(interviewId);
            Assert.That(storedReference, Is.Not.Null, "Should store questionnaire id <-> interview id relation");
        }

        [Test]
        public async Task should_handle_questionnaire_deletion()
        {
            var interviewId = Id.g1;

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                id: Id.gA,
                children: Create.TextQuestion(Id.gB));
            questionnaire.Id = $"{Id.gA:N}$1";
            questionnaire.IsDeleted = true;

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
                    EventSourceId = Id.g2,
                    Payload = new InterviewOnClientCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                },
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new TextQuestionAnswered()
                }
            };

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);

            var schemaMock = new Mock<IDatabaseSchemaService>();
            
            var filter = CreateFilter(questionnaireStorage: questionnaireStorage, databaseSchemaService: schemaMock.Object);

            // Act
            var result = await filter.FilterAsync(eventFeed);

            // Assert
            Assert.That(result, Is.Empty, "Events by deleted questionnaires should not be published for other denormalizers");
            schemaMock.Verify(x => x.DropQuestionnaireDbStructure(questionnaire), Times.Once, "Schema should be dropped for deleted questionnaire");

            var storedDeletedQuestionnaireReference = this.tenantContext.DbContext.DeletedQuestionnaires.Find(questionnaire.Id);
            Assert.That(storedDeletedQuestionnaireReference, Is.Not.Null, "Filter should remember that questionnaire schema is dropped");

            var storedInterviewReference = this.tenantContext.DbContext.InterviewReferences.Find(interviewId);
            Assert.That(storedInterviewReference, Has.Property(nameof(InterviewReference.QuestionnaireId)).EqualTo(questionnaire.Id));
        }

        [Test]
        public async Task should_not_try_to_delete_already_deleted_schema()
        {
             var interviewId = Id.g1;

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                id: Id.gA,
                children: Create.TextQuestion(Id.gB));
            questionnaire.Id = $"{Id.gA:N}$1";
            questionnaire.IsDeleted = true;

            var eventFeed = new List<Event>
            {
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new TextQuestionAnswered()
                }
            };

            // This means that questionnaire has already been deleted on previous events batch
            this.tenantContext.DbContext.DeletedQuestionnaires.Add(new DeletedQuestionnaireReference(questionnaire.Id));
            this.tenantContext.DbContext.InterviewReferences.Add(new InterviewReference
            {
                InterviewId = interviewId,
                QuestionnaireId = questionnaire.Id,
                DeletedAtUtc = DateTime.UtcNow
            });
            this.tenantContext.DbContext.SaveChanges();

            var schemaMock = new Mock<IDatabaseSchemaService>();
            var filter = CreateFilter(questionnaireStorage: Create.QuestionnaireStorage(questionnaire), databaseSchemaService: schemaMock.Object);

            // Act
            var result = await filter.FilterAsync(eventFeed);

            // Assert
            Assert.That(result, Is.Empty, "Events by deleted questionnaires should not be published for other denormalizers");
            schemaMock.Verify(x => x.DropQuestionnaireDbStructure(questionnaire), Times.Never, "Should not try to delete already deleted schema");
        }

        private DeletedQuestionnaireEventFilter CreateFilter(ITenantContext tenantContext = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IDatabaseSchemaService databaseSchemaService = null)
        {
            return new DeletedQuestionnaireEventFilter
            (
                tenantContext ?? this.tenantContext,
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                databaseSchemaService ?? Mock.Of<IDatabaseSchemaService>()
            );
        }
    }
}
