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
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class when_interview_exists_in_each_status : SurveysAndStatusesReportTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.SupervisorAssigned),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.InterviewerAssigned),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.ApprovedBySupervisor),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.RejectedBySupervisor),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.ApprovedByHeadquarters),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.RejectedByHeadquarters)
            };

            var interviewsReader = Stub.ReadSideRepository<InterviewSummary>();
            interviews.ForEach(summary => interviewsReader.Store(summary, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateSurveysAndStatusesReport(interviewsReader);
            BecauseOf();
        }

        public void BecauseOf() => report = reportFactory.Load(new SurveysAndStatusesReportInputModel());

        [NUnit.Framework.Test] public void should_return_7_in_total_count () => report.Items.First().TotalCount.Should().Be(7);

        [NUnit.Framework.Test] public void should_return_1_in_each_status ()
        {
            HeadquarterSurveysAndStatusesReportLine first = report.Items.First();
            first.SupervisorAssignedCount.Should().Be(1);
            first.InterviewerAssignedCount.Should().Be(1);
            first.CompletedCount.Should().Be(1);
            first.ApprovedBySupervisorCount.Should().Be(1);
            first.RejectedBySupervisorCount.Should().Be(1);
            first.RejectedByHeadquartersCount.Should().Be(1);
            first.ApprovedByHeadquartersCount.Should().Be(1);
        }

        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;
    }
}
