using System;
using Machine.Specifications;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Mappings;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.SupervisorTeamsAndStatusesReportTests
{
    [Subject(typeof(SupervisorTeamsAndStatusesReport))]
    internal class SupervisorTeamsAndStatusesReportContext : with_postgres_db
    {
        protected static SupervisorTeamsAndStatusesReport CreateTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> reader = null)
        {
            if (reader == null)
            {
                reader = CreateInterviewSummaryRepository();
            }
            return new SupervisorTeamsAndStatusesReport(reader);
        }
        protected static PostgreReadSideStorage<InterviewSummary> CreateInterviewSummaryRepository()
        {
            var sessionFactory = Create.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(InterviewSummaryMap), typeof(QuestionAnswerMap) });
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