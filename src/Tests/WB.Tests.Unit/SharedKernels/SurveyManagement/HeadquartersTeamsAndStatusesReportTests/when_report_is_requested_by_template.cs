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

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HeadquartersTeamsAndStatusesReportTests
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
            
            var repository = new TestInMemoryWriter<InterviewSummary>();
            interviews.ForEach(x => repository.Store(x, Guid.NewGuid().FormatGuid()));

            reportFactory = CreateTeamsAndStatusesReport(repository);
        };

        Because of = () => report = reportFactory.Load(new TeamsAndStatusesInputModel
        {
            TemplateId = questionnaireId, 
            TemplateVersion = version,
        });

        It should_count_statuses_by_questionnaire = () => report.Items.First().CompletedCount.ShouldEqual(2);

        static HeadquartersTeamsAndStatusesReport reportFactory;
        static TeamsAndStatusesReportView report;
        static Guid questionnaireId;
        static int version;
    }
}

