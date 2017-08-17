using System;
using Machine.Specifications;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.HeadquartersTeamsAndStatusesReportTests
{
    [Subject(typeof(HeadquartersTeamsAndStatusesReport))]
    internal class HeadquartersTeamsAndStatusesReportContext: with_postgres_db
    {
        protected static HeadquartersTeamsAndStatusesReport CreateTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> reader = null)
        {
            if (reader == null)
            {
                reader = CreateInterviewSummaryRepository();
            }
            return new HeadquartersTeamsAndStatusesReport(reader);
        }

        protected static PostgreReadSideStorage<InterviewSummary> CreateInterviewSummaryRepository()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(InterviewSummaryMap), typeof(QuestionAnswerMap) , typeof(InterviewStatusesMap) }, true);
            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

           return new PostgreReadSideStorage<InterviewSummary>(postgresTransactionManager, Mock.Of<ILogger>(), "InterviewId");
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
        Cleanup things = () => { pgSqlConnection.Close(); };

        protected static NpgsqlConnection pgSqlConnection;
        protected static CqrsPostgresTransactionManager postgresTransactionManager;
    }
}