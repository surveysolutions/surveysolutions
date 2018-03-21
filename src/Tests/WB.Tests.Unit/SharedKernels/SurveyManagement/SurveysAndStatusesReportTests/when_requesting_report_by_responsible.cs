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
    internal class when_requesting_report_by_responsible : SurveysAndStatusesReportTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: userId, responsibleName: responsibleName),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: userId, responsibleName: responsibleName),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: Guid.NewGuid()),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: Guid.NewGuid()),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, responsibleId: Guid.NewGuid()),
            };

            var interviewsReader = Stub.ReadSideRepository<InterviewSummary>();
            interviews.ForEach(summary => interviewsReader.Store(summary, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateSurveysAndStatusesReport(interviewsReader);
            BecauseOf();
        }

        public void BecauseOf() => report = reportFactory.Load(new SurveysAndStatusesReportInputModel { ResponsibleName = responsibleName });

        [NUnit.Framework.Test] public void should_count_only_interviews_by_responsible () => 
            report.Items.First().CompletedCount.Should().Be(2);

        static Guid userId;
        static string responsibleName = "userName";
        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;
    }
}
