using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.DeleteQuestionnaireServiceTests
{
    [TestFixture]
    public class DeleteQuestionnaireTests : with_postgres_db
    {
        private IMemoryCache memoryCache;
        private ISessionFactory factory;

        [SetUp]
        public void Setup()
        {
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            InitializeDb(DbType.Users, DbType.PlainStore, DbType.ReadSide);
            factory = IntegrationCreate.SessionFactoryProd(ConnectionStringBuilder.ConnectionString);
        }

        [Test]
        public async Task when_remove_questionnaire_then_should_clear_all_dependency()
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 5);
            var questionnaire = new QuestionnaireDocument
            {
                PublicKey = questionnaireIdentity.QuestionnaireId,
            };

            var userId = await CreateUser();
            SaveQuestionnaire(questionnaireIdentity, questionnaire);
            
            var assignmentId = CreateAssignment(questionnaireIdentity, userId);
            CreateEvents(assignmentId);

            var interviewId = CreateInterviewSummary(questionnaireIdentity, userId);
            CreateAudioFiles(interviewId);
            CreateAudioAuditFiles(interviewId);
            CreateEvents(interviewId);
            
            await DeleteQuestionnaire(questionnaire, questionnaireIdentity, userId);

            using var unitOfWork = IntegrationCreate.UnitOfWork(factory, workspace);
            var hqQuestionnaireStorage = CreateQuestionnaireStorage(unitOfWork);
            var questionnaireDocument = hqQuestionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            Assert.That(questionnaireDocument, Is.Not.Null);
            Assert.That(questionnaireDocument.IsDeleted, Is.True);

            var countInterviews = unitOfWork.Session.Query<InterviewSummary>().Count(s =>
                s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                && s.QuestionnaireVersion == questionnaireIdentity.Version);
            Assert.That(countInterviews, Is.EqualTo(0));

            var countAudio = unitOfWork.Session.Query<AudioFile>().Count(s =>
                s.InterviewId == interviewId);
            Assert.That(countAudio, Is.EqualTo(0));

            var countAudioAudit = unitOfWork.Session.Query<AudioAuditFile>().Count(s =>
                s.InterviewId == interviewId);
            Assert.That(countAudioAudit, Is.EqualTo(0));

            var countAssignments = unitOfWork.Session.Query<Assignment>().Count(a =>
                a.QuestionnaireId == questionnaireIdentity);
            Assert.That(countAssignments, Is.EqualTo(0));

            var eventStore = new PostgresEventStore(new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly), 
                unitOfWork, Mock.Of<ILogger<PostgresEventStore>>());
            var interviewEvents = eventStore.Read(interviewId, 0);
            Assert.That(interviewEvents.Count(), Is.EqualTo(0));

            var assignmentEvents = eventStore.Read(assignmentId, 0);
            Assert.That(assignmentEvents.Count(), Is.EqualTo(0));
        }

        private void CreateEvents(Guid eventSourceId)
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            var eventStore = new PostgresEventStore(new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly),
                unitOfWork,
                Mock.Of<ILogger<PostgresEventStore>>());

            eventStore.Store(new UncommittedEventStream(null, new UncommittedEvent[]
            {
                new UncommittedEvent(Guid.NewGuid(),
                    eventSourceId, 1, 0, DateTime.Now, 
                    new InterviewApproved(Guid.NewGuid(), "c", DateTimeOffset.UtcNow))
            }));
            unitOfWork.AcceptChanges();
        }

        private void CreateAudioFiles(Guid interviewId)
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            var storage = new PostgresPlainStorageRepository<AudioFile>(unitOfWork);

            var fileId = AudioFile.GetFileId(interviewId, "test");
            storage.Store(new AudioFile()
            {
                Id = fileId,
                InterviewId = interviewId,
                Data = new byte[] { 1 },
                FileName = "test"
            }, fileId);
            unitOfWork.AcceptChanges();
        }
        private void CreateAudioAuditFiles(Guid interviewId)
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            var storage = new PostgresPlainStorageRepository<AudioAuditFile>(unitOfWork);

            var fileId = AudioAuditFile.GetFileId(interviewId, "test");
            storage.Store(new AudioAuditFile()
            {
                Id = fileId,
                InterviewId = interviewId,
                Data = new byte[] { 1 },
                FileName = "test"
            }, fileId);
            unitOfWork.AcceptChanges();
        }

        private async Task DeleteQuestionnaire(QuestionnaireDocument questionnaire, 
            QuestionnaireIdentity questionnaireIdentity,
            Guid userId)
        {
            using var unitOfWorkForDelete = IntegrationCreate.UnitOfWork(factory, workspace);
            {
                var service = CreateDeleteQuestionnaireService(unitOfWorkForDelete,
                    questionnaire, questionnaireIdentity);
                await service.DeleteInterviewsAndQuestionnaireAfterAsync(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version,
                    userId);

                unitOfWorkForDelete.AcceptChanges();
            }
        }

        private HqQuestionnaireStorage CreateQuestionnaireStorage(IUnitOfWork unitOfWork)
        {
            return new HqQuestionnaireStorage(
                new PostgresKeyValueStorage<QuestionnaireDocument>(unitOfWork, new EntitySerializer<QuestionnaireDocument>()),
                new TranslationStorage(new PostgresPlainStorageRepository<TranslationInstance>(unitOfWork)),
                new QuestionnaireTranslator(), 
                new PostgreReadSideStorage<QuestionnaireCompositeItem, int>(unitOfWork, memoryCache),
                new PostgreReadSideStorage<QuestionnaireCompositeItem, int>(unitOfWork, memoryCache), 
                new QuestionnaireQuestionOptionsRepository(), 
                new SubstitutionService(), 
                Mock.Of<IInterviewExpressionStorageProvider>(), 
                Mock.Of<IReusableCategoriesFillerIntoQuestionnaire>(),
                Create.Storage.NewMemoryCache()
            );
        }

        private Guid CreateInterviewSummary(QuestionnaireIdentity questionnaireIdentity, Guid userId)
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            var interviews = new PostgreReadSideStorage<InterviewSummary, int>(unitOfWork, memoryCache);
            var interviewId = Guid.NewGuid();
            var interviewSummary = new InterviewSummary()
            {
                InterviewId = interviewId,
                AssignmentId = 2,
                QuestionnaireVariable = "var",
                ResponsibleId = userId,
                CreatedDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                FirstAnswerDate = DateTime.UtcNow,
                Status = InterviewStatus.Completed,
                Key = "11-22-33-44",
                ResponsibleName = "name",
                ResponsibleRole = UserRoles.Supervisor,
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                QuestionnaireVersion = questionnaireIdentity.Version,
                SupervisorId = userId,
            };
            interviews.Store(interviewSummary);
            unitOfWork.AcceptChanges();
            return interviewId;
        }

        private Guid CreateAssignment(QuestionnaireIdentity questionnaireIdentity, Guid userId)
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            var assignmentStorage = new PostgreReadSideStorage<Assignment, Guid>(unitOfWork, memoryCache);
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment()
            {
                Id = 2,
                QuestionnaireId = questionnaireIdentity,
                ResponsibleId = userId,
                PublicKey = assignmentId,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Answers = new List<InterviewAnswer>(),
                IdentifyingData = new List<IdentifyingAnswer>(),
            };
            assignmentStorage.Store(assignment, assignmentId);
            unitOfWork.AcceptChanges();
            return assignmentId;
        }

        private IDeleteQuestionnaireService CreateDeleteQuestionnaireService(IUnitOfWork unitOfWork, 
            QuestionnaireDocument questionnaire, QuestionnaireIdentity questionnaireIdentity)
        {
            var interviewsToDeleteFactory = new InterviewsToDeleteFactory(unitOfWork,
                Mock.Of<IImageFileStorage>(),
                Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<ILogger<InterviewsToDeleteFactory>>());

            IPlainStorageAccessor<TranslationInstance> translations =
                new PostgresPlainStorageRepository<TranslationInstance>(unitOfWork);
            var translationManagementService = new TranslationManagementService(
                translations);

            var questionnaireBrowseItem = new PostgresPlainStorageRepository<QuestionnaireBrowseItem>(unitOfWork);

            var lookupTablesStorage = new PostgresKeyValueStorage<QuestionnaireLookupTable>(unitOfWork,
                new EntitySerializer<QuestionnaireLookupTable>());
            var hqQuestionnaireStorage = CreateQuestionnaireStorage(unitOfWork);
            var questionnaireBackupStorage = new PostgresKeyValueStorage<QuestionnaireBackup>(unitOfWork,

                new EntitySerializer<QuestionnaireBackup>());
            var commandService = new Mock<ICommandService>();
            commandService.Setup(c => c.Execute(It.IsAny<DeleteQuestionnaire>(), null))
                .Callback(() =>
                {
                    questionnaire.IsDeleted = true;
                    hqQuestionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                        questionnaireIdentity.Version,
                        questionnaire);
                });


            IDeleteQuestionnaireService service = new DeleteQuestionnaireService(
                interviewsToDeleteFactory,
                commandService.Object,
                Mock.Of<ILogger<DeleteQuestionnaireService>>(),
                translationManagementService,
                Mock.Of<IAssignmentsImportService>(),
                Mock.Of<ISystemLog>(),
                questionnaireBrowseItem,
                lookupTablesStorage,
                hqQuestionnaireStorage,
                null,
                new InvitationsDeletionService(unitOfWork),
                Mock.Of<IAggregateRootCache>(),
                new AssignmentsToDeleteFactory(unitOfWork, Mock.Of<ILogger<AssignmentsToDeleteFactory>>()),
                new ReusableCategoriesStorage(new PostgresPlainStorageRepository<ReusableCategoricalOptions>(unitOfWork)),
                questionnaireBackupStorage,
                Mock.Of<IExportServiceApi>());
            return service;
        }

        private void SaveQuestionnaire(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire)
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory, workspace);

            var hqQuestionnaireStorage = CreateQuestionnaireStorage(unitOfWork);

            hqQuestionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                questionnaire);

            var questionnaireBrowseItem = new PostgresPlainStorageRepository<QuestionnaireBrowseItem>(unitOfWork);

            questionnaireBrowseItem.Store(new QuestionnaireBrowseItem()
            {
                Id = questionnaireIdentity.ToString(),
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                Version = questionnaireIdentity.Version,
                Title = "test",
                Variable = "var",
                CreationDate = DateTime.UtcNow,
            }, questionnaireIdentity.ToString());
            unitOfWork.AcceptChanges();
        }

        private async Task<Guid> CreateUser()
        {
            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            var usersStorage = new HqUserStore(unitOfWork, new IdentityErrorDescriber());
            var user = new HqUser()
            {
                IsArchived = false,
                UserName = "name",
                CreationDate = DateTime.UtcNow,
            };
            user.WorkspaceProfile = null;
            //user.Roles.Add(new HqRole() { Id = Guid.NewGuid(), Name = "Hq" });
            await usersStorage.CreateAsync(user);
            var userId = usersStorage.Users.Single().Id;
            unitOfWork.AcceptChanges();
            
            return userId;
        }
    }
}
