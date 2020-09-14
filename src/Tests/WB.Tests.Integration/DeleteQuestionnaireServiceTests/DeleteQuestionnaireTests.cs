using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.DeleteQuestionnaireServiceTests
{
    [TestFixture]
    public class DeleteQuestionnaireTests : with_postgres_db
    {
        [Test]
        public async Task when_remove_questionnaire_then_should_clear_all_dependency()
        {
            var userId = Guid.NewGuid();
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 5);
            
            InitializeDb(DbType.PlainStore, DbType.ReadSide);

            var unitOfWorkConnectionSettings = new UnitOfWorkConnectionSettings()
            {
                ConnectionString = ConnectionStringBuilder.ConnectionString
            };
            
            var factoryUsers = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new List<Type>()
                {
                    typeof(HqUserMap),
                    typeof(HqUserClaimMap),
                    typeof(DeviceSyncInfoMap),
                    typeof(HqUserProfileMap),
                    typeof(HqRoleMap),
                    typeof(SyncStatisticsMap),
                }, true, "users"
            );
            using var usersUnitOfWork = IntegrationCreate.UnitOfWork(factoryUsers);
            var usersStorage = new HqUserStore(usersUnitOfWork, new IdentityErrorDescriber());
            await usersStorage.CreateAsync(new HqUser()
            {
                Id = userId,
                IsArchived = false,
                UserName = "name",
            });
            await usersUnitOfWork.Session.FlushAsync();
            
            var factory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                unitOfWorkConnectionSettings.ReadSideSchemaName, 
                new List<Type>
                {
                    typeof(InterviewSummaryMap),
                    typeof(QuestionnaireCompositeItemMap),
                    typeof(QuestionAnswerMap),
                    typeof(InterviewStatisticsReportRowMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(InterviewGpsMap),
                    typeof(CumulativeReportStatusChangeMap),
                    typeof(InterviewCommentedStatusMap),
                    typeof(InterviewCommentMap),
                    typeof(AssignmentMap),
                    typeof(ReadonlyUserMap),
                    typeof(QuestionnaireLiteViewItemMap),
                    typeof(ProfileMap),
                },
                unitOfWorkConnectionSettings.PlainStorageSchemaName, 
                new List<Type>
                {
                    typeof(AudioAuditFileMap),
                    typeof(AudioFileMap),
                    typeof(QuestionnaireBrowseItemMap),
                },true);

            using var unitOfWork = IntegrationCreate.UnitOfWork(factory);

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            
            var interviewsToDeleteFactory = new InterviewsToDeleteFactory(unitOfWork,
                Mock.Of<IImageFileStorage>(),
                Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<Microsoft.Extensions.Logging.ILogger>());

            IPlainStorageAccessor<TranslationInstance> translations = new PostgresPlainStorageRepository<TranslationInstance>(unitOfWork);
            var translationManagementService = new TranslationManagementService(
                translations);

            var questionnaireBrowseItemReader = new PostgresPlainStorageRepository<QuestionnaireBrowseItem>(unitOfWork);

            var lookupTablesStorage = new PostgresPlainKeyValueStorage<QuestionnaireLookupTable>(unitOfWork,
                unitOfWorkConnectionSettings, 
                Mock.Of<ILogger>(),
                memoryCache,
                new EntitySerializer<QuestionnaireLookupTable>());
            var hqQuestionnaireStorage = new HqQuestionnaireStorage(
                new PostgresPlainKeyValueStorage<QuestionnaireDocument>(unitOfWork, unitOfWorkConnectionSettings, 
                    Mock.Of<ILogger>(), memoryCache, new EntitySerializer<QuestionnaireDocument>()),
                new TranslationStorage(translations),
                new QuestionnaireTranslator(), 
                new PostgreReadSideStorage<QuestionnaireCompositeItem, int>(unitOfWork, memoryCache),
                new PostgreReadSideStorage<QuestionnaireCompositeItem, int>(unitOfWork, memoryCache), 
                new QuestionnaireQuestionOptionsRepository(), 
                new SubstitutionService(), 
                Mock.Of<IInterviewExpressionStatePrototypeProvider>(), 
                Mock.Of<IReusableCategoriesFillerIntoQuestionnaire>()
                );
            var questionnaireBackupStorage = new PostgresPlainKeyValueStorage<QuestionnaireBackup>(unitOfWork,
                unitOfWorkConnectionSettings, 
                Mock.Of<ILogger>(),
                memoryCache,
                new EntitySerializer<QuestionnaireBackup>());
            IDeleteQuestionnaireService service = new DeleteQuestionnaireService(
                interviewsToDeleteFactory, 
                Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                translationManagementService,
                Mock.Of<IAssignmentsImportService>(), 
                Mock.Of<ISystemLog>(),
                questionnaireBrowseItemReader,
                lookupTablesStorage,
                hqQuestionnaireStorage,
                null,
                new InvitationsDeletionService(unitOfWork), 
                Mock.Of<IAggregateRootCache>(),
                new AssignmentsToDeleteFactory(unitOfWork, Mock.Of<Microsoft.Extensions.Logging.ILogger>()), 
                new ReusableCategoriesStorage(new PostgresPlainStorageRepository<ReusableCategoricalOptions>(unitOfWork)), 
                questionnaireBackupStorage
                );
            
            await unitOfWork.Session.FlushAsync();

            var questionnaire = new QuestionnaireDocument();
            
            hqQuestionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                questionnaire);
            
            questionnaireBrowseItemReader.Store(new QuestionnaireBrowseItem()
            {
                Id = questionnaireIdentity.ToString(),
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                Version = questionnaireIdentity.Version,
                Title = "test",
                CreationDate = DateTime.UtcNow,
            }, questionnaireIdentity.ToString());
            await unitOfWork.Session.FlushAsync();
            
            var assignmentStorage = new PostgreReadSideStorage<Assignment, Guid>(unitOfWork, memoryCache);
            var assignmentId = Guid.NewGuid();
            assignmentStorage.Store(new Assignment()
            {
                Id = 2,
                QuestionnaireId = questionnaireIdentity,
                ResponsibleId = userId, 
                PublicKey = assignmentId,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Answers = new List<InterviewAnswer>(),
                IdentifyingData = new List<IdentifyingAnswer>(),
            }, assignmentId);

            var interviews = new PostgreReadSideStorage<InterviewSummary, int>(unitOfWork, memoryCache);
            var interviewId = Guid.NewGuid();
            var interviewSummary = new InterviewSummary()
            {
                Id = 3,
                InterviewId = interviewId,
                AssignmentId = 2,
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
            interviews.Store(interviewSummary, 3);

            await unitOfWork.Session.FlushAsync();

            await service.DeleteInterviewsAndQuestionnaireAfterAsync(questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                userId);

            await unitOfWork.Session.FlushAsync();

            var questionnaireDocument = hqQuestionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            Assert.That(questionnaireDocument, Is.Not.Null);
            Assert.That(questionnaireDocument.IsDeleted, Is.True);
        }
    }
}