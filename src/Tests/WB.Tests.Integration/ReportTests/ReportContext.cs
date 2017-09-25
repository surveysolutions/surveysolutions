﻿using System.Collections.Generic;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.ReportTests
{
    internal abstract class ReportContext : NpgsqlTestContext
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
            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(InterviewSummaryMap), typeof(TimeSpanBetweenStatusesMap), typeof(QuestionAnswerMap) }, true);
            transactionManager = new CqrsPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

           return new PostgreReadSideStorage<InterviewSummary>(transactionManager, Mock.Of<ILogger>(), "InterviewId");
        }

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
