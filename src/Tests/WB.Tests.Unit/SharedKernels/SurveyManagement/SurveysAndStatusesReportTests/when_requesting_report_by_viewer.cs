﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveysAndStatusesReportTests
{
    internal class when_requesting_report_by_team_lead: SurveysAndStatusesReportTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, teamLeadId: userId, teamLeadName: teamLeadName),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, teamLeadId: userId, teamLeadName: teamLeadName),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, teamLeadId: Guid.NewGuid()),
            };

            var interviewsReader = Stub.ReadSideRepository<InterviewSummary>();
            interviews.ForEach(summary => interviewsReader.Store(summary, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateSurveysAndStatusesReport(interviewsReader);
        };

        Because of = () => report = reportFactory.Load(new SurveysAndStatusesReportInputModel { TeamLeadName = teamLeadName });

        It should_count_only_interviews_by_teamlead = () =>
            report.Items.First().CompletedCount.ShouldEqual(2);

        static Guid userId;
        static string teamLeadName = "userName";
        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;     
    }
}