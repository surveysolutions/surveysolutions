using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class QuestionnaireEventFilterTests
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

            var questionnaireVersion = 1;
            var questionnaire = Create.QuestionnaireDocument(
                id: Id.gA,
                version: questionnaireVersion,
                children: Create.TextQuestion(Id.gB));
            
            var eventFeed = new List<Event>
            {
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new InterviewCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = questionnaireVersion
                    }
                },
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new InterviewOnClientCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = questionnaireVersion
                    }
                }

            };

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage);

            // Act
            var result = await filter.FilterAsync(eventFeed);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2), "Both InterviewCreated and InterviewOnClientCreated should be published to denormalizers");

            var storedReference = dbContext.InterviewReferences.Find(interviewId);
            Assert.That(storedReference, Is.Not.Null, "Should store questionnaire id <-> interview id relation");
        }

        [Test]
        public async Task should_create_interview_reference_from_InterviewFromPreloadedDataCreated()
        {
            var interviewId = Id.g1;

            var questionnaireVersion = 1;
            var questionnaire = Create.QuestionnaireDocument(
                id: Id.gA,
                version:questionnaireVersion,
                children: Create.TextQuestion(Id.gB));

            var eventFeed = new List<Event>
            {
                new Event
                {
                    EventSourceId = interviewId,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = questionnaireVersion
                    }
                }
            };

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage);

            // Act
            await filter.FilterAsync(eventFeed);

            // Assert
            var storedReference = dbContext.InterviewReferences.Find(interviewId);
            Assert.That(storedReference, Is.Not.Null, "Should store questionnaire id <-> interview id relation");
        }

        [Test]
        public async Task should_generate_schema_for_questionnaire_once_on_questionnaire_request()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: Id.gA);
            questionnaire.QuestionnaireId = new QuestionnaireIdentity($"{Id.gA:N}$1");
            questionnaire.Id = questionnaire.QuestionnaireId.ToString();

            var eventFeed = CrateEventFeedWith3InterviewCreatedEvents(questionnaire);

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);
            var questionnaireSchemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();
            var databaseSchemaService = Create.DatabaseSchemaService(questionnaireSchemaGenerator.Object, dbContext);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage, databaseSchemaService: databaseSchemaService);

            // Act
            await filter.FilterAsync(eventFeed);

            // Assert
            questionnaireSchemaGenerator.Verify(q => q.CreateQuestionnaireDbStructure(questionnaire), Times.Once);
        }

        [Test]
        public async Task should_drop_schema_for_questionnaire_once_questionnaire_is_deleted()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: Id.gA);
            questionnaire.QuestionnaireId = new QuestionnaireIdentity($"{Id.gA:N}$1");
            questionnaire.Id = questionnaire.QuestionnaireId.ToString();

            var eventFeed = CrateEventFeedWith3InterviewCreatedEvents(questionnaire);

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);
            var questionnaireSchemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();
            var databaseSchemaService = Create.DatabaseSchemaService(questionnaireSchemaGenerator.Object, dbContext);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage, databaseSchemaService: databaseSchemaService);
            await filter.FilterAsync(eventFeed);

            questionnaireSchemaGenerator.Verify(q => q.CreateQuestionnaireDbStructure(questionnaire), Times.Once);

            // mark questionnaire as deleted on 
            questionnaire.IsDeleted = true;

            // Act
            await filter.FilterAsync(new List<Event>
            {
                new Event
                {
                    EventSourceId = Id.g1,
                    Payload = new InterviewHardDeleted()
                },
                new Event
                {
                    EventSourceId = Id.g2,
                    Payload = new InterviewHardDeleted()
                }
            });

            questionnaireSchemaGenerator.Verify(q => q.DropQuestionnaireDbStructure(questionnaire), Times.Once);
        }

        [Test]
        public async Task should_generate_schema_for_cached_questionnaire_when_db_do_not_hold_reference_on_questionnaire()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: Id.gA);
            questionnaire.QuestionnaireId = new QuestionnaireIdentity($"{Id.gA:N}$1");
            questionnaire.Id = questionnaire.QuestionnaireId.ToString();

            var eventFeed = CrateEventFeedWith3InterviewCreatedEvents(questionnaire);

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);
            var questionnaireSchemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();
            var databaseSchemaService = Create.DatabaseSchemaService(questionnaireSchemaGenerator.Object, dbContext);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage, databaseSchemaService: databaseSchemaService);
            await filter.FilterAsync(eventFeed);

            // caching questionnaire
            await questionnaireStorage.GetQuestionnaireAsync(questionnaire.QuestionnaireId);

            // create questionnaire SHOULD happen
            questionnaireSchemaGenerator.Verify(q => q.CreateQuestionnaireDbStructure(questionnaire), Times.Once);
            questionnaireSchemaGenerator.Reset();

            // emulate schema drop while Export Service is running
            var reference = this.dbContext.GeneratedQuestionnaires.Find(questionnaire.Id);
            dbContext.GeneratedQuestionnaires.Remove(reference);
            dbContext.SaveChanges();

            await filter.FilterAsync(new List<Event>
            {
                new Event
                {
                    EventSourceId = Id.g4,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                },
                new Event
                {
                    EventSourceId = Id.g5,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                },
                new Event
                {
                    EventSourceId = Id.g6,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                }
            });

            questionnaireSchemaGenerator.Verify(q => q.CreateQuestionnaireDbStructure(questionnaire), Times.Once);
        }

        [Test]
        public async Task should_invalidate_questionnaire_storage_when_received_interview_deleted_flag()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: Id.gA);
            questionnaire.QuestionnaireId = new QuestionnaireIdentity($"{Id.gA:N}$1");
            questionnaire.Id = questionnaire.QuestionnaireId.ToString();

            var eventFeed = CrateEventFeedWith3InterviewCreatedEvents(questionnaire);

            var questionnaireStorage = Create.QuestionnaireStorage(questionnaire);
            var questionnaireSchemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();
            var databaseSchemaService = Create.DatabaseSchemaService(questionnaireSchemaGenerator.Object, dbContext);

            var filter = CreateFilter(questionnaireStorage: questionnaireStorage, databaseSchemaService: databaseSchemaService);
            await filter.FilterAsync(eventFeed);

            questionnaireSchemaGenerator.Verify(q => q.CreateQuestionnaireDbStructure(questionnaire), Times.Once);

            // mark questionnaire as deleted on 
            questionnaire.IsDeleted = true;

            // Act
            await filter.FilterAsync(new List<Event>
            {
                new Event
                {
                    EventSourceId = Id.g1,
                    Payload = new InterviewHardDeleted()
                },
                new Event
                {
                    EventSourceId = Id.g2,
                    Payload = new InterviewHardDeleted()
                }
            });

            Mock.Get(questionnaireStorage).Verify(q => q.InvalidateQuestionnaire(questionnaire.QuestionnaireId, null), Times.Exactly(2));
            questionnaireSchemaGenerator.Verify(q => q.DropQuestionnaireDbStructure(questionnaire), Times.Once);
        }


        [Test]
        public void should_drop_schema_only_once_by_database_schema_service()
        {
            var questionnaire = Create.QuestionnaireDocument();
            questionnaire.IsDeleted = true;

            var questionnaireSchemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();
            var databaseSchemaService = Create.DatabaseSchemaService(questionnaireSchemaGenerator.Object, dbContext);

            databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            databaseSchemaService.CreateOrRemoveSchema(questionnaire);

            questionnaireSchemaGenerator.Verify(q => q.DropQuestionnaireDbStructure(questionnaire), Times.Once);
        }

        [Test]
        public void should_generate_schema_only_once_by_database_schema_service()
        {
            var questionnaire = Create.QuestionnaireDocument();
            var questionnaireSchemaGenerator = new Mock<IQuestionnaireSchemaGenerator>();
            var databaseSchemaService = Create.DatabaseSchemaService(questionnaireSchemaGenerator.Object, dbContext);

            databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            databaseSchemaService.CreateOrRemoveSchema(questionnaire);
            databaseSchemaService.CreateOrRemoveSchema(questionnaire);

            questionnaireSchemaGenerator.Verify(q => q.CreateQuestionnaireDbStructure(questionnaire), Times.Once);
        }

        private QuestionnaireEventFilter CreateFilter(ITenantContext tenantContext = null,
            IQuestionnaireStorage questionnaireStorage = null, 
            IDatabaseSchemaService databaseSchemaService = null)
        {
            return new QuestionnaireEventFilter
            (dbContext,
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                databaseSchemaService ?? Mock.Of<IDatabaseSchemaService>()
            );
        }

        private static List<Event> CrateEventFeedWith3InterviewCreatedEvents(QuestionnaireDocument questionnaire)
        {
            return new List<Event>
            {
                new Event
                {
                    EventSourceId = Id.g1,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                },
                new Event
                {
                    EventSourceId = Id.g2,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                },
                new Event
                {
                    EventSourceId = Id.g3,
                    Payload = new InterviewFromPreloadedDataCreated
                    {
                        QuestionnaireId = questionnaire.PublicKey,
                        QuestionnaireVersion = 1
                    }
                }
            };
        }
    }
}
