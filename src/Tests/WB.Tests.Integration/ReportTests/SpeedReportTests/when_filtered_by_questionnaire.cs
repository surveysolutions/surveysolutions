using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.SpeedReportTests
{
    internal class when_filtered_by_questionnaire : SpeedReportContext
    {
        [SetUp]
        public void Setup()
        {
            SetupSessionFactory();
        }

        [Test]
        public void should_find_by_questionnaire_id()
        {
            var supervisorId = Id.gA;
            var interviewerId = Id.g1;
            var reportEndDate = new DateTime(2010, 10, 30);

            var interview = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(10), supervisorId, interviewerId, reportEndDate, questionnaireId: Id.g2, questionnaireVersion: 1);
            var interview1 = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(8), supervisorId, interviewerId, reportEndDate, questionnaireId: Id.g2, questionnaireVersion: 2);
            var interview2 = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(10), supervisorId, interviewerId, reportEndDate, questionnaireId: Id.g3);
            var interviewSummaries =  CreateInterviewSummaryRepository();

            interview = AppendSpeedReportInfo(interview);
            interview1 = AppendSpeedReportInfo(interview1);
            interview2 = AppendSpeedReportInfo(interview2);

            interviewSummaries.Store(interview, interview.SummaryId);
            interviewSummaries.Store(interview1, interview1.SummaryId);
            interviewSummaries.Store(interview2, interview2.SummaryId);

            var report = CreateSpeedReport(interviewSummaries);

            // Act
            var speedBySupervisorsReportInputModel = new SpeedBySupervisorsReportInputModel
            {
                InterviewStatuses = new[] {InterviewExportedAction.Completed},
                ColumnCount = 1,
                Period = "d",
                From = reportEndDate,
                QuestionnaireId = Id.g2
            };

            var reportValue = report.Load(speedBySupervisorsReportInputModel);

            // Assert
            Assert.That(reportValue.Items.FirstOrDefault().Average, Is.EqualTo(9));
        }

        [Test]
        public void should_find_by_questionnaire_version()
        {
            var supervisorId = Id.gA;
            var interviewerId = Id.g1;
            var reportEndDate = new DateTime(2010, 10, 30);

            var interview = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(10), supervisorId, interviewerId, reportEndDate, questionnaireId: Id.g2, questionnaireVersion: 1);
            var interview1 = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(8), supervisorId, interviewerId, reportEndDate, questionnaireId: Id.g2, questionnaireVersion: 2);
            var interviewSummaries =  CreateInterviewSummaryRepository();

            interview = AppendSpeedReportInfo(interview);
            interview1 = AppendSpeedReportInfo(interview1);

            interviewSummaries.Store(interview, interview.SummaryId);
            interviewSummaries.Store(interview1, interview1.SummaryId);

            var report = CreateSpeedReport(interviewSummaries);

            // Act
            var speedBySupervisorsReportInputModel = new SpeedBySupervisorsReportInputModel
            {
                InterviewStatuses = new[] {InterviewExportedAction.Completed},
                ColumnCount = 1,
                Period = "d",
                From = reportEndDate,
                QuestionnaireId = Id.g2,
                QuestionnaireVersion = 1
            };

            var reportValue = report.Load(speedBySupervisorsReportInputModel);

            // Assert
            Assert.That(reportValue.Items.FirstOrDefault().Average, Is.EqualTo(10));
        }
    }
}
