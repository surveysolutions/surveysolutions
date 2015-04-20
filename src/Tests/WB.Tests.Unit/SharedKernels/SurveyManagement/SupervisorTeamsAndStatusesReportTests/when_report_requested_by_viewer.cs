using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorTeamsAndStatusesReportTests
{
    internal class when_report_requested_by_viewer : SupervisorTeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            viewerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            List<InterviewSummary> interviews = new List<InterviewSummary>
            {
                Create.InterviewSummary(teamLeadId: viewerId, status: InterviewStatus.Completed),
                Create.InterviewSummary(teamLeadId: viewerId, status: InterviewStatus.Completed),
                Create.InterviewSummary(teamLeadId: Guid.NewGuid(), status: InterviewStatus.Completed),
            };

            var repository = new TestInMemoryWriter<InterviewSummary>();
            interviews.ForEach(x => repository.Store(x, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateTeamsAndStatusesReport(repository);
        };

        Because of = () => report = reportFactory.Load(new TeamsAndStatusesInputModel { ViewerId = viewerId });

        It should_count_number_of_interviews_for_teamlead = () => report.Items.First().CompletedCount.ShouldEqual(2);

        static SupervisorTeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid viewerId;
    }
}

