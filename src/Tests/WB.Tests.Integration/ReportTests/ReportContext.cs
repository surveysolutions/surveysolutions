using System;
using System.Collections.Generic;
using System.Configuration;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.ReportTests
{
    internal abstract class ReportContext
    {
        internal ReportContext()
        {
            Sv = new SvReport(this);
            Hq = new HqReport(this);
        }

        public readonly SvReport Sv;
        public readonly HqReport Hq;

        protected PostgreReadSideStorage<InterviewSummary> CreateInterviewSummaryRepository()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(InterviewSummaryMap), typeof(QuestionAnswerMap) }, true);
            transactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

           return new PostgreReadSideStorage<InterviewSummary>(transactionManager, Mock.Of<ILogger>(), "InterviewId");
        }

        protected NpgsqlConnection pgSqlConnection;
        protected CqrsPostgresTransactionManager transactionManager;

        [SetUp]
        public void Context()
        {
            TestConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = $@"CREATE DATABASE {databaseName} ENCODING = 'UTF8'";
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        [TearDown]
        public void Cleanup()
        {
            pgSqlConnection.Close();

            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = string.Format(
                    @"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'; DROP DATABASE {0};",
                    databaseName);
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void ExecuteInCommandTransaction(Action action)
        {
            try
            {
                transactionManager.BeginCommandTransaction();

                action();

                transactionManager.CommitCommandTransaction();
            }
            catch
            {
                transactionManager.RollbackCommandTransaction();
                throw;
            }
        }


        protected static NpgsqlConnectionStringBuilder connectionStringBuilder;
        protected static string TestConnectionString;
        private static string databaseName;

        internal class SvReport
        {
            private readonly ReportContext reportContext;

            public SvReport(ReportContext reportContext)
            {
                this.reportContext = reportContext;
            }

            public TeamsAndStatusesReport TeamsAndStatuses(INativeReadSideStorage<InterviewSummary> reader = null)
            {
                if (reader == null)
                {
                    reader = reportContext.CreateInterviewSummaryRepository();
                }
                return new TeamsAndStatusesReport(reader);
            }


            public SurveysAndStatusesReport SurveyAndStatuses(INativeReadSideStorage<InterviewSummary> reader = null)
            {
                if (reader == null)
                {
                    reader = reportContext.CreateInterviewSummaryRepository();
                }
                return new SurveysAndStatusesReport(reader);
            }

            internal SurveysAndStatusesReport SurveyAndStatuses(List<InterviewSummary> interviews)
            {
                var reader = reportContext.CreateInterviewSummaryRepository();
                reportContext.ExecuteInCommandTransaction(() => interviews.ForEach(x => reader.Store(x, x.InterviewId.FormatGuid())));
                return SurveyAndStatuses(reader);
            }
        }

        internal class HqReport
        {
            private readonly ReportContext reportContext;

            public HqReport(ReportContext reportContext)
            {
                this.reportContext = reportContext;
            }

            public TeamsAndStatusesReport TeamsAndStatuses(INativeReadSideStorage<InterviewSummary> reader = null)
            {
                if (reader == null)
                {
                    reader = reportContext.CreateInterviewSummaryRepository();
                }
                return new TeamsAndStatusesReport(reader);
            }

            public SurveysAndStatusesReport SurveyAndStatuses(INativeReadSideStorage<InterviewSummary> reader = null)
            {
                if (reader == null)
                {
                    reader = reportContext.CreateInterviewSummaryRepository();
                }
                return new SurveysAndStatusesReport(reader);
            }

            public SurveysAndStatusesReport SurveyAndStatuses(List<InterviewSummary> interviews)
            {
                var reader = reportContext.CreateInterviewSummaryRepository();
                reportContext.ExecuteInCommandTransaction(() => interviews.ForEach(x => reader.Store(x, x.InterviewId.FormatGuid())));
                return SurveyAndStatuses(reader);
            }
        }
    }
}
