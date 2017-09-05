using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_loading_report_with_data_in_last_timespan : QuantityReportFactoryTestContext
    {
        [Test]
        public void should_include_data_in_last_column()
        {
            var reportStartDate = new DateTime(2017, 5, 10, 9, 0, 0);
            var supervisorId = Id.g1;

            var input = CreateQuantityBySupervisorsReportInputModel(period: "d", columnCount: 1, from: reportStartDate);

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses.Store(
                Create.Entity.InterviewStatuses(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(supervisorId: supervisorId, 
                            timestamp: reportStartDate)
                    }), "2");

            var quantityReportFactory = CreateQuantityReportFactory(interviewStatuses: interviewStatuses);

            // Act
            var report = quantityReportFactory.Load(input);

            // Assert
            report.Items.Should().HaveCount(1);
        }
    }
}