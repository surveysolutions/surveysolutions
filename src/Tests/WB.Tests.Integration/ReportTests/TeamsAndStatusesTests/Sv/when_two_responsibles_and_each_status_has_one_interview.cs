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
    internal class when_two_responsibles_and_each_status_has_one_interview : TeamsAndStatusesReportContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid firstResponsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid secondResponsibleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.SupervisorAssigned, questionnaireId: questionnaireId),
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.InterviewerAssigned, questionnaireId: questionnaireId),
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.Completed, questionnaireId: questionnaireId),
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.ApprovedBySupervisor, questionnaireId: questionnaireId),
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.RejectedBySupervisor, questionnaireId: questionnaireId),
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.ApprovedByHeadquarters, questionnaireId: questionnaireId),
                Abc.Create.Entity.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.RejectedByHeadquarters, questionnaireId: questionnaireId),

                Abc.Create.Entity.InterviewSummary(responsibleId: secondResponsibleId, status: InterviewStatus.Completed, questionnaireId: questionnaire1Id),
                Abc.Create.Entity.InterviewSummary(responsibleId: secondResponsibleId, status: InterviewStatus.Completed, questionnaireId: questionnaire1Id),
                Abc.Create.Entity.InterviewSummary(responsibleId: secondResponsibleId, status: InterviewStatus.Completed, questionnaireId: questionnaire1Id),
            };

            var repository = CreateInterviewSummaryRepository();
            ExecuteInCommandTransaction(() => interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid())));

            reportFactory = CreateSvTeamsAndStatusesReport(repository);
            BecauseOf();
        }

        public void BecauseOf() => report = reportFactory.GetBySupervisorAndDependentInterviewers(new TeamsAndStatusesInputModel
        {
            Order = "CompletedCount ASC"
        });

        [NUnit.Framework.Test] public void should_return_row_per_responsible () => report.TotalCount.Should().Be(2);

        [NUnit.Framework.Test] public void should_return_1_in_each_status_for_first_responsible () 
        {
            var firstLine = report.Items.First();
            firstLine.SupervisorAssignedCount.Should().Be(1);
            firstLine.InterviewerAssignedCount.Should().Be(1);
            firstLine.CompletedCount.Should().Be(1);
            firstLine.ApprovedBySupervisorCount.Should().Be(1);
            firstLine.RejectedBySupervisorCount.Should().Be(1);
            firstLine.ApprovedByHeadquartersCount.Should().Be(1);
            firstLine.RejectedByHeadquartersCount.Should().Be(1);
        }

        [NUnit.Framework.Test] public void should_count_3_interviews_for_second_responsible () => 
            report.Items.ToArray()[1].CompletedCount.Should().Be(3);

        static TeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        static Guid questionnaire1Id = Guid.Parse("22222222222222222222222222222222");
    }
}
