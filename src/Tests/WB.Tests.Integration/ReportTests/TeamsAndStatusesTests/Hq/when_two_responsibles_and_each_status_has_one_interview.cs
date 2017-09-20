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

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Hq
{
    internal class when_two_responsibles_and_each_status_has_one_interview : TeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            Guid firstTeamLeadId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid secondTeamLeadId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.SupervisorAssigned),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.InterviewerAssigned),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.ApprovedBySupervisor),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.RejectedBySupervisor),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.ApprovedByHeadquarters),
                Abc.Create.Entity.InterviewSummary(teamLeadId: firstTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.RejectedByHeadquarters),

                Abc.Create.Entity.InterviewSummary(teamLeadId: secondTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: secondTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Abc.Create.Entity.InterviewSummary(teamLeadId: secondTeamLeadId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
            };

            var repository = CreateInterviewSummaryRepository();
            ExecuteInCommandTransaction(() => interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid())));

            reportFactory = CreateHqTeamsAndStatusesReport(repository);
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.GetBySupervisors(new TeamsAndStatusesInputModel {Order = "CompletedCount ASC" }));

        It should_return_row_per_responsible = () => report.TotalCount.ShouldEqual(2);

        It should_return_1_in_each_status_for_first_responsible = () =>
        {
            var firstLine = report.Items.First();
            firstLine.SupervisorAssignedCount.ShouldEqual(1);
            firstLine.InterviewerAssignedCount.ShouldEqual(1);
            firstLine.CompletedCount.ShouldEqual(1);
            firstLine.ApprovedBySupervisorCount.ShouldEqual(1);
            firstLine.RejectedBySupervisorCount.ShouldEqual(1);
            firstLine.ApprovedByHeadquartersCount.ShouldEqual(1);
            firstLine.RejectedByHeadquartersCount.ShouldEqual(1);
        };

        It should_count_3_interviews_for_second_responsible = () => report.Items.ToList()[1].CompletedCount.ShouldEqual(3);

        static TeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
    }
}