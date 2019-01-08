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
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
        private HashSet<QuestionnaireIdentity> questionnairesList;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            connectionString = DatabaseTestInitializer.InitializeDb(DbType.ReadSide);

            sessionFactory = IntegrationCreate.SessionFactory(
                this.connectionString, new List<Type> { typeof(CumulativeReportStatusChangeMap) }, 
                true, schemaName: new UnitOfWorkConnectionSettings().ReadSideSchemaName);

            UnitOfWork = NewUnitOfWork();
            cumulativeReportStatusChangeStorage = new PostgreReadSideStorage<CumulativeReportStatusChange>(
                UnitOfWork, 
                Mock.Of<ILogger>(), 
                Mock.Of<IServiceLocator>());

            questionnairesList = new HashSet<QuestionnaireIdentity>();
            var mock = new Mock<IAllUsersAndQuestionnairesFactory>();
            
            mock.Setup(s => s.GetQuestionnaires(It.IsAny<string>(), It.IsAny<long?>()))
                .Returns<Guid?, long?>((id, ver) => questionnairesList.ToList());

            this.questionnaires = mock.Object;
        }
        
        protected IUnitOfWork NewUnitOfWork() => IntegrationCreate.UnitOfWork(sessionFactory);

        protected IUnitOfWork UnitOfWork { get; set; }

        private static long eventSequence = 0;
        private static long NextSequence => Interlocked.Increment(ref eventSequence);

        protected ChartStatisticsViewFactory CreateChartStatisticsViewFactory()
        {
            return new ChartStatisticsViewFactory(UnitOfWork, questionnaires);
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
            this.questionnairesList.Remove(questionnaireId);
        }

        protected void EnsureQuestionnaireExists(QuestionnaireIdentity questionnaireId)
        {
            this.questionnairesList.Add(questionnaireId);
        }

        protected void AddInterviewStatuses(
            QuestionnaireIdentity questionnaireId, Guid interviewId,
            params (DateTime date, InterviewStatus status)[] statuses)
        {
            EnsureQuestionnaireExists(questionnaireId);

            InterviewStatus? last = null;
            Guid? lastEntryId = null;

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

        private IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private IAllUsersAndQuestionnairesFactory questionnaires;

        protected void AddStatusChangeLine(Guid entryId, QuestionnaireIdentity questionnaireId, Guid interviewId, DateTime date, InterviewStatus status, int changeValue)
        {
            var entry = entryId + (changeValue > 0 ? "_plus" : "_minus");

            cumulativeReportStatusChangeStorage.Store(new CumulativeReportStatusChange(
                entry, questionnaireId.QuestionnaireId, questionnaireId.Version, date, status, changeValue, interviewId, NextSequence)
                , entryId);
        }
    }
    
}
