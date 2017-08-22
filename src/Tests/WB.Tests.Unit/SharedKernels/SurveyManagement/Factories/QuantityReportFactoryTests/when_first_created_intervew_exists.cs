using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_first_created_intervew_exists : QuantityReportFactoryTestContext
    {
        [Test]
        public void should_limit_selected_period_with_first_interview_created_date()
        {
            var reportStartDate = new DateTime(2017, 5, 10);

            var input = CreateQuantityBySupervisorsReportInputModel(period: "d", columnCount: 4, from: reportStartDate);

            var user = Guid.NewGuid();

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatuses.Store(
                Create.Entity.InterviewStatuses(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: new[]
                    {
                        Create.Entity.InterviewCommentedStatus(timestamp: reportStartDate.AddDays(-2)),
                        Create.Entity.InterviewCommentedStatus(supervisorId: user,
                            timestamp: reportStartDate.AddHours(-1))
                    }), "2");

            var quantityReportFactory = CreateQuantityReportFactory(interviewStatuses: interviewStatuses);

            // Act
            var report = quantityReportFactory.Load(input);

            // Assert
            report.Items.Should().HaveCount(2);
        }
    }
}