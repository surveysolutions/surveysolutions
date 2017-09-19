using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_getting_report_with_header : QuantityReportFactoryTestContext
    {
        [Test]
        public void should_include_data_in_last_column()
        {
            var reportStartDate = new DateTime(2017, 5, 10, 9, 0, 0);
            var supervisorId = Id.g1;

            var input = CreateQuantityBySupervisorsReportInputModel(period: "m", columnCount: 1, from: reportStartDate);

            var interviewStatuses = new TestInMemoryWriter<InterviewSummary>();
            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(supervisorId: supervisorId, 
                            timestamp: reportStartDate)
                    }), "2");

            var quantityReportFactory = CreateQuantityReportFactory(interviewStatuses: interviewStatuses);

            // Act
            var report = quantityReportFactory.GetReport(input);

            // Assert
            report.Headers.Second().ShouldEqual(reportStartDate.ToString("yyyy-MM-dd"));
        }
    }
}