using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Sv
{
    internal class when_report_requested_by_viewer : TeamsAndStatusesReportContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        public void BecauseOf() => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.GetBySupervisorAndDependentInterviewers(new TeamsAndStatusesInputModel { ViewerId = viewerId }));

        [NUnit.Framework.Test] public void should_count_number_of_interviews_for_teamlead () => report.Items.First().CompletedCount.Should().Be(2);

        static TeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid viewerId;
    }
}

