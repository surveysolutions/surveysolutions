using System;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.TeamInterviewsFactoryTests
{
    internal class TeamInterviewsFactoryTestContext 
    {
        public static ITeamInterviewsFactory CreateTeamInterviewsFactory(
            out PostgreReadSideStorage<InterviewSummary> reader,
            out PostgreReadSideStorage<QuestionAnswer> featuredQuestionAnswersReader)
        {
            connectionString = DatabaseTestInitializer.InitializeDb(DbType.ReadSide);

            var sessionFactory = IntegrationCreate.SessionFactory(connectionString, new[]
            {
                typeof(InterviewSummaryMap), typeof(TimeSpanBetweenStatusesMap), typeof(QuestionAnswerMap), typeof(InterviewCommentedStatusMap)
            }, true, schemaName: "readside");

            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionString);
            pgSqlConnection.Open();

            reader = new PostgreReadSideStorage<InterviewSummary>(postgresTransactionManager, Mock.Of<ILogger>(), "InterviewId");
            featuredQuestionAnswersReader = new PostgreReadSideStorage<QuestionAnswer>(postgresTransactionManager, Mock.Of<ILogger>(), "Questionid");

            return new TeamInterviewsFactory(
                reader ?? CreateInterviewSummaryRepository());
        }
        protected static PostgreReadSideStorage<InterviewSummary> CreateInterviewSummaryRepository()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionString, new[] { typeof(InterviewSummaryMap), typeof(TimeSpanBetweenStatusesMap), typeof(QuestionAnswerMap) }, true);
            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionString);
            pgSqlConnection.Open();

            return new PostgreReadSideStorage<InterviewSummary>(postgresTransactionManager, Mock.Of<ILogger>(), "InterviewId");
        }

        protected static PostgreReadSideStorage<QuestionAnswer> CreateQuestionAnswerRepository()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionString, new[] { typeof(InterviewSummaryMap), typeof(TimeSpanBetweenStatusesMap), typeof(QuestionAnswerMap) }, true);
            postgresTransactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionString);
            pgSqlConnection.Open();

            return new PostgreReadSideStorage<QuestionAnswer>(postgresTransactionManager, Mock.Of<ILogger>(), "Questionid");
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
            DatabaseTestInitializer.DropDb(connectionString);
        }

        protected static NpgsqlConnection pgSqlConnection;
        protected static CqrsPostgresTransactionManager postgresTransactionManager;
        private static string connectionString;
    }
}
