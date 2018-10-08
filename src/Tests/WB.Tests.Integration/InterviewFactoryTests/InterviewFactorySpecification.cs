using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Repositories;
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
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.InterviewFactoryTests
{
    internal class InterviewFactorySpecification
    {
        private string connectionString;
        protected PostgreReadSideStorage<InterviewSummary> interviewSummaryRepository;
        protected PostgreReadSideStorage<QuestionnaireCompositeItem, int> questionnaireItemsRepository;
        protected HqQuestionnaireStorage questionnaireStorage;
        protected InMemoryKeyValueStorage<QuestionnaireDocument> questionnaireDocumentRepository;

        protected IUnitOfWork UnitOfWork;
        protected ISessionFactory sessionFactory;


        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.connectionString = DatabaseTestInitializer.InitializeDb(DbType.PlainStore, DbType.ReadSide);

            sessionFactory = IntegrationCreate.SessionFactory(this.connectionString,
                new List<Type>
                {
                    typeof(InterviewSummaryMap),
                    typeof(QuestionnaireCompositeItemMap),
                    typeof(QuestionAnswerMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(InterviewCommentedStatusMap)
                }, true, new UnitOfWorkConnectionSettings().ReadSideSchemaName);

            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<int[][]>>(new EntitySerializer<int[][]>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<GeoPosition>>(new EntitySerializer<GeoPosition>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<InterviewTextListAnswer[]>>(new EntitySerializer<InterviewTextListAnswer[]>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<AnsweredYesNoOption[]>>(new EntitySerializer<AnsweredYesNoOption[]>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<AudioAnswer>>(new EntitySerializer<AudioAnswer>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<Area>>(new EntitySerializer<Area>());
        }

        [SetUp]
        public void EachTestSetup()
        {
            this.UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);

            this.interviewSummaryRepository = new PostgreReadSideStorage<InterviewSummary>(this.UnitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());
            this.questionnaireItemsRepository = new PostgreReadSideStorage<QuestionnaireCompositeItem, int>(this.UnitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());
            this.questionnaireDocumentRepository = new InMemoryKeyValueStorage<QuestionnaireDocument>();
            this.questionnaireStorage = new HqQuestionnaireStorage(new InMemoryKeyValueStorage<QuestionnaireDocument>(),
                Mock.Of<ITranslationStorage>(), Mock.Of<IQuestionnaireTranslator>(),
                this.questionnaireItemsRepository, Mock.Of<IQuestionOptionsRepository>(),
                Mock.Of<ISubstitutionService>());
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
            using (var unitOfWork = IntegrationCreate.UnitOfWork(sessionFactory))
            {
                var interviewSummaryRepository = new PostgreReadSideStorage<InterviewSummary>(unitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());
                interviewSummary.QuestionnaireIdentity = questionnaireIdentity.ToString();
                interviewSummary.SummaryId = interviewSummary.InterviewId.FormatGuid();
                interviewSummaryRepository.Store(interviewSummary, interviewSummary.SummaryId);
                unitOfWork.AcceptChanges();
            }
        }

        protected void PrepareQuestionnaire(QuestionnaireDocument document, long questionnaireVersion = 1)
        {
            using (var unitOfWork = IntegrationCreate.UnitOfWork(sessionFactory))
            {
                var questionnaireItemsRepositoryLocal = new PostgreReadSideStorage<QuestionnaireCompositeItem, int>(unitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());
                
                var questionnaireStorageLocal = new HqQuestionnaireStorage(new InMemoryKeyValueStorage<QuestionnaireDocument>(),
                    Mock.Of<ITranslationStorage>(), Mock.Of<IQuestionnaireTranslator>(),
                    questionnaireItemsRepositoryLocal, Mock.Of<IQuestionOptionsRepository>(),
                    Mock.Of<ISubstitutionService>());

                document.Id = document.PublicKey.FormatGuid();
                questionnaireStorageLocal.StoreQuestionnaire(document.PublicKey, questionnaireVersion, document);
                unitOfWork.AcceptChanges();
            }
        }

        protected InterviewEntity[] GetInterviewEntities(InterviewFactory factory, Guid interviewId, Guid questionnaireId, long version = 1) =>
                factory.GetInterviewEntities(interviewId).ToArray();

        protected InterviewEntity[] GetInterviewEntities(InterviewFactory factory, QuestionnaireIdentity questionnaireId, Guid interviewId) =>
                factory.GetInterviewEntities(interviewId).ToArray();

        protected InterviewFactory CreateInterviewFactory()
        {
            return new InterviewFactory(
                summaryRepository: interviewSummaryRepository,
                sessionProvider: this.UnitOfWork);
        }
    }
}
