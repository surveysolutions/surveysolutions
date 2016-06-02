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

namespace WB.Tests.Integration.HeadquartersTeamsAndStatusesReportTests
{
    internal class when_report_is_requested_by_template_with_version : HeadquartersTeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            Guid teamLeadId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            version = 1;

            List<InterviewSummary> interviews = new List<InterviewSummary>
            {
                Create.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: questionnaireId, questionnaireVersion: version, status: InterviewStatus.Completed),
                Create.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: questionnaireId, questionnaireVersion: version, status: InterviewStatus.Completed),
                Create.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: questionnaireId, questionnaireVersion: 2, status: InterviewStatus.Completed),
                Create.InterviewSummary(teamLeadId: teamLeadId, questionnaireId: Guid.NewGuid(), questionnaireVersion: version, status: InterviewStatus.Completed),
            };

            var repository = CreateInterviewSummaryRepository();
            ExecuteInCommandTransaction(() => interviews.ForEach(x => repository.Store(x, x.InterviewId.FormatGuid())));

            reportFactory = CreateTeamsAndStatusesReport(repository);
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(()=>reportFactory.Load(new TeamsAndStatusesInputModel
        {
            TemplateId = questionnaireId, 
            TemplateVersion = version,
        }));

        It should_count_statuses_by_questionnaire = () => report.Items.First().CompletedCount.ShouldEqual(2);

        static HeadquartersTeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid questionnaireId;
        static int version;
    }
}

