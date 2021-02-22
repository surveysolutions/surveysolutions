using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

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

        [OneTimeSetUp]
        public void Init()
        {
            DatabaseTestInitializer.InitializeDb(connectionStringBuilder.ConnectionString, Create.Service.WorkspaceContextAccessor(), DbType.ReadSide, DbType.PlainStore);
        }

        protected PostgreReadSideStorage<InterviewSummary> SetupAndCreateInterviewSummaryRepository()
        {
            SetupSessionFactory();
            return CreateInterviewSummaryRepository();
        }

        protected void SetupSessionFactory()
        {

            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString,
                new[]
                {
                    typeof(InterviewSummaryMap),
                    typeof(InterviewGpsMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(IdentifyEntityValueMap),
                    typeof(InterviewStatisticsReportRowMap),
                    typeof(InterviewCommentedStatusMap),
                    typeof(InterviewCommentMap),
                    typeof(QuestionnaireCompositeItemMap),
                    typeof(InterviewReportAnswerMap)
                }, true, Create.Service.WorkspaceContextAccessor().CurrentWorkspace().SchemaName);

            UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
        }

        protected PostgreReadSideStorage<InterviewSummary> CreateInterviewSummaryRepository()
        {
            return IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
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
                    reader = reportContext.SetupAndCreateInterviewSummaryRepository();
                }
                return new TeamsAndStatusesReport(reader);
            }


            public SurveysAndStatusesReport SurveyAndStatuses(INativeReadSideStorage<InterviewSummary> reader = null)
            {
                if (reader == null)
                {
                    reader = reportContext.SetupAndCreateInterviewSummaryRepository();
                }
                return new SurveysAndStatusesReport(reader, reportContext.UnitOfWork);
            }

            internal SurveysAndStatusesReport SurveyAndStatuses(List<InterviewSummary> interviews)
            {
                var reader = reportContext.SetupAndCreateInterviewSummaryRepository();
                interviews.ForEach(x => reader.Store(x, x.InterviewId.FormatGuid()));
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
                    reader = reportContext.SetupAndCreateInterviewSummaryRepository();
                }
                return new TeamsAndStatusesReport(reader);
            }

            public SurveysAndStatusesReport SurveyAndStatuses(INativeReadSideStorage<InterviewSummary> reader = null)
            {
                if (reader == null)
                {
                    reader = reportContext.SetupAndCreateInterviewSummaryRepository();
                }
                return new SurveysAndStatusesReport(reader, reportContext.UnitOfWork);
            }

            public SurveysAndStatusesReport SurveyAndStatuses(List<InterviewSummary> interviews)
            {
                var reader = reportContext.SetupAndCreateInterviewSummaryRepository();
                interviews.ForEach(x => reader.Store(x, x.InterviewId.FormatGuid()));
                return SurveyAndStatuses(reader);
            }
        }

    }
}
