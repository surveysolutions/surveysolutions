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
using WB.Tests.Integration.PostgreSQLTests;

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
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString, new[]
            {
                typeof(InterviewSummaryMap),
                typeof(TimeSpanBetweenStatusesMap),
                typeof(QuestionAnswerMap),
                typeof(InterviewCommentedStatusMap)
            }, true);
            UnitOfWork = Mock.Of<IUnitOfWork>(x => x.Session == sessionFactory.OpenSession());

            pgSqlConnection = new NpgsqlConnection(ConnectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            return new PostgreReadSideStorage<InterviewSummary>(UnitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());
        }

        protected static void ExecuteInCommandTransaction(Action action)
        {
            action();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            pgSqlConnection.Close();
        }

        protected static NpgsqlConnection pgSqlConnection;
        protected static IUnitOfWork UnitOfWork;
    }
}
