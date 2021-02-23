using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.ReusableCategories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.InterviewFactoryTests
{
    internal class InterviewFactorySpecification
    {
        private string connectionString;
        protected PostgreReadSideStorage<InterviewSummary> interviewSummaryRepository;

        protected IUnitOfWork UnitOfWork;
        protected ISessionFactory sessionFactory;
        protected IWorkspaceContextAccessor workspace;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SetUp.MockedServiceLocator();
            
            this.connectionString = DatabaseTestInitializer.CreateAndInitializeDb(DbType.PlainStore, DbType.ReadSide);

            workspace = Create.Service.WorkspaceContextAccessor();
            sessionFactory = IntegrationCreate.SessionFactory(this.connectionString,
                new List<Type>
                {
                    typeof(InterviewSummaryMap),
                    typeof(QuestionnaireCompositeItemMap),
                    typeof(IdentifyEntityValueMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(InterviewStatisticsReportRowMap),
                    typeof(CumulativeReportStatusChangeMap),
                    typeof(InterviewCommentedStatusMap),
                    typeof(InterviewFlagMap),
                    typeof(InterviewGpsMap),
                    typeof(InterviewCommentMap)
                }, true, workspace.CurrentWorkspace().SchemaName);

            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<int[][]>>(new EntitySerializer<int[][]>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<InterviewTextListAnswer[]>>(new EntitySerializer<InterviewTextListAnswer[]>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<AnsweredYesNoOption[]>>(new EntitySerializer<AnsweredYesNoOption[]>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<AudioAnswer>>(new EntitySerializer<AudioAnswer>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<Area>>(new EntitySerializer<Area>());
        }

        [SetUp]
        public void EachTestSetup()
        {
            this.UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
            this.interviewSummaryRepository = IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
        }

        [TearDown]
        public void TearDown()
        {

            this.UnitOfWork.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DatabaseTestInitializer.DropDb(this.connectionString);
        }

        protected void StoreInterviewSummary(InterviewSummary interviewSummary, QuestionnaireIdentity questionnaireIdentity)
        {
            var interviewSummaryRepository = IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
            interviewSummary.QuestionnaireIdentity = questionnaireIdentity.ToString();
            interviewSummary.SummaryId = interviewSummary.InterviewId.FormatGuid();

            interviewSummaryRepository.Store(interviewSummary, interviewSummary.SummaryId);
        }

        protected IQuestionnaireStorage PrepareQuestionnaire(QuestionnaireDocument document, long questionnaireVersion = 1)
        {
            var questionnaireItemsRepositoryLocal = 
                IntegrationCreate.PostgresReadSideRepository<QuestionnaireCompositeItem, int>(UnitOfWork);

            var reusableCategoriesFillerIntoQuestionnaire = new Mock<IReusableCategoriesFillerIntoQuestionnaire>();
            reusableCategoriesFillerIntoQuestionnaire
                .Setup(x => x.FillCategoriesIntoQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>(), It.IsAny<QuestionnaireDocument>()))
                .Returns<QuestionnaireIdentity, QuestionnaireDocument>((identity, doc) => doc);

            var questionnaireStorageLocal = new HqQuestionnaireStorage(new InMemoryKeyValueStorage<QuestionnaireDocument>(),
                Mock.Of<ITranslationStorage>(), 
                Mock.Of<IQuestionnaireTranslator>(),
                questionnaireItemsRepositoryLocal,
                questionnaireItemsRepositoryLocal, 
                Mock.Of<IQuestionOptionsRepository>(),
                Mock.Of<ISubstitutionService>(),
                Create.Service.ExpressionStatePrototypeProvider(),
                reusableCategoriesFillerIntoQuestionnaire.Object,
                Create.Storage.NewMemoryCache());

            document.Id = document.PublicKey.FormatGuid();
            questionnaireStorageLocal.StoreQuestionnaire(document.PublicKey, questionnaireVersion, document);

            return questionnaireStorageLocal;
        }

        protected InterviewFactory CreateInterviewFactory()
        {
            return new InterviewFactory(sessionProvider: this.UnitOfWork, Mock.Of<IAuthorizedUser>());
        }

        protected List<Answer> GetAnswersFromEnum<T>(params T[] exclude) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<object>();
            return values
                .Where(v => exclude.All(e => (int)(object)e != (int)v))
                .Select(v => Create.Entity.Answer(v.ToString(), (int)v)).ToList();
        }
    }
}
