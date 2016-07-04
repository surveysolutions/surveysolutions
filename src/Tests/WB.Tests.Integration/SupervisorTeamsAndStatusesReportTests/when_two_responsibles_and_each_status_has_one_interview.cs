﻿using System;
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

namespace WB.Tests.Integration.SupervisorTeamsAndStatusesReportTests
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
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.SupervisorAssigned),
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.InterviewerAssigned),
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.Completed),
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.ApprovedBySupervisor),
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.RejectedBySupervisor),
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.ApprovedByHeadquarters),
                Create.InterviewSummary(responsibleId: firstResponsibleId, status: InterviewStatus.RejectedByHeadquarters),
                                        
                Create.InterviewSummary(responsibleId: secondResponsibleId, status: InterviewStatus.Completed),
                Create.InterviewSummary(responsibleId: secondResponsibleId, status: InterviewStatus.Completed),
                Create.InterviewSummary(responsibleId: secondResponsibleId, status: InterviewStatus.Completed),
            };

            var repository = CreateInterviewSummaryRepository();
            ExecuteInCommandTransaction(() => interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid())));

            reportFactory = CreateTeamsAndStatusesReport(repository);
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.Load(new TeamsAndStatusesInputModel() {Order = "CompletedCount ASC" }));

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

        static SupervisorTeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
    }
}