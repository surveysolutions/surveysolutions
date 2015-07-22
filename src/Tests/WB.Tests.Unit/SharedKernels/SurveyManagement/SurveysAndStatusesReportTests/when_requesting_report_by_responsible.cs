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
    internal class when_requesting_report_by_responsible : SurveysAndStatusesReportTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: userId),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: userId),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: Guid.NewGuid()),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: Guid.NewGuid()),
                Create.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: Guid.NewGuid()),
            };

            var interviewsReader = Stub.ReadSideRepository<InterviewSummary>();
            interviews.ForEach(summary => interviewsReader.Store(summary, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateSurveysAndStatusesReport(interviewsReader);
        };

        Because of = () => report = reportFactory.Load(new SurveysAndStatusesReportInputModel { ResponsibleId = userId });

        It should_count_only_interviews_by_responsible = () => 
            report.Items.First().CompletedCount.ShouldEqual(2);

        static Guid userId;
        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;
    }
}