using System;
using Moq;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;
using with_postgres_db = WB.Tests.Integration.PostgreSQLTests.with_postgres_db;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests
{
    [TestOf(typeof(TeamsAndStatusesReport))]
    internal class TeamsAndStatusesReportContext: with_postgres_db
    {
        protected static TeamsAndStatusesReport CreateHqTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> reader = null)
        {
            if (reader == null)
            {
                reader = CreateInterviewSummaryRepository();
            }
            return new TeamsAndStatusesReport(reader);
        }
        
        protected static TeamsAndStatusesReport CreateSvTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> reader = null)
        {
            if (reader == null)
            {
                reader = CreateInterviewSummaryRepository();
            }
            return new TeamsAndStatusesReport(reader);
        }

        protected static PostgreReadSideStorage<InterviewSummary> CreateInterviewSummaryRepository()
        {
            var workspaceNameProvider = Create.Service.WorkspaceContextAccessor();
            DatabaseTestInitializer.InitializeDb(ConnectionStringBuilder.ConnectionString, workspaceNameProvider, DbType.ReadSide, DbType.PlainStore);

            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString, new[]
            {
                typeof(InterviewSummaryMap),
                typeof(TimeSpanBetweenStatusesMap),
                typeof(IdentifyEntityValueMap),
                typeof(InterviewStatisticsReportRowMap),
                typeof(InterviewCommentedStatusMap),
                typeof(InterviewCommentMap),
                typeof(InterviewGpsMap),
                typeof(QuestionnaireCompositeItemMap)
                
            }, true, workspaceNameProvider.CurrentWorkspace().SchemaName);

            UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
            return IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
        }

        protected static void ExecuteInCommandTransaction(Action action)
        {
            action();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            UnitOfWork.Dispose();
        }

        protected static IUnitOfWork UnitOfWork;
    }
}
