using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class ChartStatisticsViewFactoryTestsContext 
    {
        private string connectionString;
        private ISessionFactory sessionFactory;
        protected IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            connectionString = DatabaseTestInitializer.CreateAndInitializeDb(DbType.ReadSide, DbType.PlainStore);

            sessionFactory = IntegrationCreate.SessionFactory(
                this.connectionString, 
                new List<Type>
                {
                    typeof(CumulativeReportStatusChangeMap),
                    typeof(InterviewSummaryMap),
                    typeof(IdentifyEntityValueMap),
                    typeof(InterviewCommentedStatusMap),
                    typeof(InterviewCommentMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(InterviewGpsMap),
                    typeof(InterviewStatisticsReportRowMap),
                    typeof(QuestionnaireCompositeItemMap),
                }, 
                true, Create.Service.WorkspaceContextAccessor().CurrentWorkspace().SchemaName);

            UnitOfWork = NewUnitOfWork();
            cumulativeReportStatusChangeStorage =
                IntegrationCreate.PostgresReadSideRepository<CumulativeReportStatusChange>(UnitOfWork);
            interviewSummary =
                IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
            
            var mock = new Mock<IAllUsersAndQuestionnairesFactory>();
            
            mock.Setup(s => s.GetQuestionnaires(It.IsAny<Guid?>(), It.IsAny<long?>()))
                .Returns<Guid?, long?>((id, ver) 
                    => this.questionnaireRepository
                        .Query(_ => _.Where(q => q.IsDeleted == false)
                            .Select(q => QuestionnaireIdentity.Parse(q.Id)).ToList()));
            this.questionnaires = mock;

            this.questionnaireRepository = Create.Storage.InMemoryPlainStorage<QuestionnaireBrowseItem>();
        }
        
        protected IUnitOfWork NewUnitOfWork() => IntegrationCreate.UnitOfWork(sessionFactory);

        protected IUnitOfWork UnitOfWork { get; set; }

        private static long eventSequence = 0;
        private static long NextSequence => Interlocked.Increment(ref eventSequence);

        protected ChartStatisticsViewFactory CreateChartStatisticsViewFactory()
        {
            return new ChartStatisticsViewFactory(UnitOfWork, questionnaires.Object, 
                cumulativeReportStatusChangeStorage, 
                questionnaireRepository);
        }

        protected void CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(QuestionnaireIdentity questionnaireId, DateTime date, int count)
        {
            for(int i = 0; i < count; i++)
            {
                AddInterviewStatuses(questionnaireId, Guid.NewGuid(), (date, InterviewStatus.Completed));
                AddInterviewStatuses(questionnaireId, Guid.NewGuid(), (date, InterviewStatus.Completed),
                    (date, InterviewStatus.ApprovedBySupervisor));

                AddInterviewStatuses(questionnaireId, Guid.NewGuid(), 
                    (date, InterviewStatus.Completed), 
                    (date, InterviewStatus.ApprovedBySupervisor), 
                    (date, InterviewStatus.ApprovedByHeadquarters));

                AddInterviewStatuses(questionnaireId, Guid.NewGuid(), 
                    (date, InterviewStatus.Completed), 
                    (date, InterviewStatus.ApprovedBySupervisor), 
                    (date, InterviewStatus.RejectedByHeadquarters));

                AddInterviewStatuses(questionnaireId, Guid.NewGuid(), 
                    (date, InterviewStatus.Completed), 
                    (date, InterviewStatus.RejectedBySupervisor));
            }
        }

        protected void MarkQuestionnaireDeleted(QuestionnaireIdentity questionnaireId)
        {
            var q = this.questionnaireRepository.GetById(questionnaireId.Id);
            q.IsDeleted = true;
            this.questionnaireRepository.Store(q ,q.Id);
        }

        protected void EnsureQuestionnaireExists(QuestionnaireIdentity questionnaireId)
        {
            var q = this.questionnaireRepository.GetById(questionnaireId.Id);
            if (q == null)
            {
                this.questionnaireRepository.Store(new QuestionnaireBrowseItem
                {
                    Id = questionnaireId.Id,
                    Title = questionnaireId.Id,
                    Version = questionnaireId.Version,
                    QuestionnaireId = questionnaireId.QuestionnaireId
                }, questionnaireId.Id);
            }
        }

        protected void AddInterviewStatuses(
            QuestionnaireIdentity questionnaireId, Guid interviewId,
            params (DateTime date, InterviewStatus status)[] statuses)
        {
            EnsureQuestionnaireExists(questionnaireId);

            InterviewStatus? last = null;
            Guid? lastEntryId = null;
            
            interviewSummary.Store(new InterviewSummary()
            {
                InterviewId = interviewId,
                QuestionnaireId = questionnaireId.QuestionnaireId,
                QuestionnaireVersion = questionnaireId.Version,
                QuestionnaireVariable = interviewId.ToString(),
                CreatedDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            }, interviewId);

            foreach (var status in statuses)
            {
                var entry = Guid.NewGuid();
                if (last != null)
                {
                    AddStatusChangeLine(lastEntryId.Value, questionnaireId, interviewId, status.date, last.Value, -1);
                }

                last = status.status;
                lastEntryId = entry;
                AddStatusChangeLine(entry, questionnaireId, interviewId, status.date, status.status, +1);
            }

            this.UnitOfWork.Session.Flush();            
        }

        protected void AddInterviewStatuses(QuestionnaireIdentity questionnaireId, DateTime date,
            params InterviewStatus[][] statuses)
        {
            foreach (var interviewStatuses in statuses)
            {
                AddInterviewStatuses(questionnaireId, Guid.NewGuid(), interviewStatuses.Select(i => (date, i)).ToArray());
            }
        }

        private PostgreReadSideStorage<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private PostgreReadSideStorage<InterviewSummary> interviewSummary;
        protected Mock<IAllUsersAndQuestionnairesFactory> questionnaires;

        protected void AddStatusChangeLine(Guid entryId, QuestionnaireIdentity questionnaireId, Guid interviewId, DateTime date, InterviewStatus status, int changeValue)
        {
            var entry = entryId + (changeValue > 0 ? "_plus" : "_minus");

            cumulativeReportStatusChangeStorage.Store(new CumulativeReportStatusChange(
                entry, questionnaireId.QuestionnaireId, questionnaireId.Version, date, status, changeValue, interviewId, NextSequence)
                , entryId);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            UnitOfWork.Dispose();
            DatabaseTestInitializer.DropDb(connectionString);
        }
    }
    
}
