using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorTeamsAndStatusesReportTests
{
    internal class when_two_responsibles_and_each_status_has_one_interview : SupervisorTeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            Guid firstResponsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid secondResponsibleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.SupervisorAssigned),
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.InterviewerAssigned),
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.ApprovedBySupervisor),
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.RejectedBySupervisor),
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.ApprovedByHeadquarters),
                Create.InterviewSummary(responsibleId: firstResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.RejectedByHeadquarters),
                                        
                Create.InterviewSummary(responsibleId: secondResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Create.InterviewSummary(responsibleId: secondResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
                Create.InterviewSummary(responsibleId: secondResponsibleId, questionnaireId: questionnaireId, status: InterviewStatus.Completed),
            };

            var repository = new TestInMemoryWriter<InterviewSummary>();
            interviews.ForEach(x => repository.Store(x, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateTeamsAndStatusesReport(repository);
        };

        Because of = () => report = reportFactory.Load(new TeamsAndStatusesInputModel { });

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

        It should_count_3_interviews_for_second_responsible = () => report.Items.Second().CompletedCount.ShouldEqual(3);

        static SupervisorTeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
    }
}