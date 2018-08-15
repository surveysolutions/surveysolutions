using System;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
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
            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(ConnectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            return new PostgreReadSideStorage<InterviewSummary>(postgresTransactionManager, Mock.Of<ILogger>());
        }

        protected static void ExecuteInCommandTransaction(Action action)
        {
            try
            {
                postgresTransactionManager.BeginCommandTransaction();

                action();

                postgresTransactionManager.CommitCommandTransaction();
            }
            catch
            {
                postgresTransactionManager.RollbackCommandTransaction();
                throw;
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            pgSqlConnection.Close();
        }

        protected static NpgsqlConnection pgSqlConnection;
        protected static CqrsPostgresTransactionManager postgresTransactionManager;
    }
}
