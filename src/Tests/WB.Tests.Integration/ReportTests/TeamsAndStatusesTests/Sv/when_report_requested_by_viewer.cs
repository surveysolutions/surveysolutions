using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Hq;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Sv
{
    internal class when_report_requested_by_viewer : TeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            viewerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var responsible = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            List<InterviewSummary> interviews = new List<InterviewSummary>
            {
                Abc.Create.Entity.InterviewSummary(responsibleId: responsible, teamLeadId: viewerId, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(responsibleId: responsible, teamLeadId: viewerId, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(responsibleId: responsible, teamLeadId: Guid.NewGuid(), status: InterviewStatus.Completed),
            };

            var repository = CreateInterviewSummaryRepository();
            ExecuteInCommandTransaction(() => interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid())));

            reportFactory = CreateSvTeamsAndStatusesReport(repository);
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.Load(new TeamsAndStatusesInputModel { ViewerId = viewerId }));

        It should_count_number_of_interviews_for_teamlead = () => report.Items.First().CompletedCount.ShouldEqual(2);

        static SupervisorTeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid viewerId;
    }
}

