using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
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
        Establish context = () =>
        {
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
        };

        Because of = () => report = reportFactory.Load(new SurveysAndStatusesReportInputModel());

        It should_return_7_in_total_count = () => report.Items.First().TotalCount.ShouldEqual(7);

        It should_return_1_in_each_status = () =>
        {
            HeadquarterSurveysAndStatusesReportLine first = report.Items.First();
            first.SupervisorAssignedCount.ShouldEqual(1);
            first.InterviewerAssignedCount.ShouldEqual(1);
            first.CompletedCount.ShouldEqual(1);
            first.ApprovedBySupervisorCount.ShouldEqual(1);
            first.RejectedBySupervisorCount.ShouldEqual(1);
            first.RejectedByHeadquartersCount.ShouldEqual(1);
            first.ApprovedByHeadquartersCount.ShouldEqual(1);
        };

        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;
    }
}