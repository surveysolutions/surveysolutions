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

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Hq
{
     internal class when_report_is_requested_by_template : TeamsAndStatusesReportContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid teamLeadId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var version = 1;

            List<InterviewSummary> interviews = new List<InterviewSummary>
            {
                Abc.Create.Entity.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: questionnaireId, questionnaireVersion: version, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: questionnaireId, questionnaireVersion: version, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: questionnaireId, questionnaireVersion: version + 1, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: Guid.NewGuid(), questionnaireVersion: version, status: InterviewStatus.Completed),
            };

            var repository = CreateInterviewSummaryRepository();
            interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid()));

            reportFactory = CreateHqTeamsAndStatusesReport(repository);

            BecauseOf();
        }

        public void BecauseOf() => report = reportFactory.GetBySupervisors(new TeamsAndStatusesByHqInputModel
        {
            TemplateId = questionnaireId
        });

        [NUnit.Framework.Test] public void should_count_statuses_by_questionnaire () => report.Items.First().CompletedCount.Should().Be(3);
        [NUnit.Framework.Test] public void should_fill_total_row() => report.TotalCount.Should().Be(1);

        static TeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid questionnaireId;
    }
}

