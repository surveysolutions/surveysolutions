using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.SpeedReportTests
{
    [TestFixture]
    internal class should_calculate_report_for_supervisor : SpeedReportContext
    {
        [SetUp]
        public void Setup()
        {
            SetupSessionFactory();
        }

        [Test]
        public void should_calculate_interview_duration_for_supervisor_with_one_interviewer()
        {
            var supervisorId = Id.gA;
            var interviewerId = Id.g1;
            var reportEndDate = new DateTime(2010, 10, 30);

            var interview = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(15), 
                supervisorId, interviewerId, reportEndDate);
            var interviewSummaries = CreateInterviewSummaryRepository();

            interview = AppendSpeedReportInfo(interview);

            interviewSummaries.Store(interview, interview.SummaryId);

            var report = CreateSpeedReport(interviewSummaries);

            UnitOfWork.Session.Flush();

            // Act
            var speedBySupervisorsReportInputModel = new SpeedBySupervisorsReportInputModel
            {
                InterviewStatuses = new[] {InterviewExportedAction.Completed},
                ColumnCount = 1,
                Period = "d",
                From = reportEndDate
            };

            var reportValue = report.Load(speedBySupervisorsReportInputModel);

            // Assert
            Assert.That(reportValue.Items.FirstOrDefault().Average, Is.EqualTo(15));
        }

        [Test]
        public void should_calculate_duration_for_two_interviewers_in_same_team()
        {
            var supervisorId = Id.gA;
            var interviewer1Id = Id.g1;
            var interviewer2Id = Id.g2;
            var reportEndDate = new DateTime(2010, 10, 30);

            var firstInterviewDuration = 15;
            var secondInterviewDuration = 9;
            var interview1 = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(firstInterviewDuration), supervisorId, interviewer1Id, reportEndDate);
            var interview2 = CreateCompletedInterviewWithDuration(TimeSpan.FromMinutes(secondInterviewDuration), supervisorId, interviewer2Id, reportEndDate);

            var interviewSummaries = CreateInterviewSummaryRepository();
            interview1 = AppendSpeedReportInfo(interview1);
            interview2 = AppendSpeedReportInfo(interview2);

            interviewSummaries.Store(interview1, interview1.SummaryId);
            interviewSummaries.Store(interview2, interview2.SummaryId);

            var report = CreateSpeedReport(interviewSummaries);

            // Act
            var speedBySupervisorsReportInputModel = new SpeedBySupervisorsReportInputModel
            {
                InterviewStatuses = new[] {InterviewExportedAction.Completed},
                ColumnCount = 1,
                Period = "d",
                From = reportEndDate
            };

            var reportValue = report.Load(speedBySupervisorsReportInputModel);

            // Assert
            Assert.That(reportValue.Items.FirstOrDefault().Average, Is.EqualTo((firstInterviewDuration + secondInterviewDuration) / 2));
        }
    }
}
