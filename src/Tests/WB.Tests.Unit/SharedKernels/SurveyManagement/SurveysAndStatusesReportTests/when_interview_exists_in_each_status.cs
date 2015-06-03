using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveysAndStatusesReportTests
{
    internal class when_interview_exists_in_each_status : SurveysAndStatusesReportTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.SupervisorAssigned),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.InterviewerAssigned),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.ApprovedBySupervisor),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.RejectedBySupervisor),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.ApprovedByHeadquarters),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.RejectedByHeadquarters)
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