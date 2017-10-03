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
using WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Hq;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Sv
{
    internal class when_two_responsibles_and_each_status_has_one_interview : TeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.GetBySupervisorAndDependentInterviewers(new TeamsAndStatusesInputModel
        {
            Order = "CompletedCount ASC"
        }));

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

        It should_count_3_interviews_for_second_responsible = () => report.Items.ToArray()[1].CompletedCount.ShouldEqual(3);

        static TeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        static Guid questionnaire1Id = Guid.Parse("22222222222222222222222222222222");
    }
}