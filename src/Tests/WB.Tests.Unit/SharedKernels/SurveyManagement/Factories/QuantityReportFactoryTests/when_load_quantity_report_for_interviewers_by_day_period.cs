using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_report_for_interviewers_by_day_period : QuantityReportFactoryTestContext
    {
        private QuantityReportFactory quantityReportFactory;
        private QuantityByInterviewersReportInputModel input;
        private QuantityByResponsibleReportView result;
        private TestInMemoryWriter<InterviewSummary> interviewStatuses;
        private readonly Guid supervisorId = Guid.Parse("11111111111111111111111111111111");

        [OneTimeSetUp]
        public void context()
        {
            input = CreateQuantityByInterviewersReportInputModel(supervisorId: supervisorId);

            var user = Guid.NewGuid();
            var userFromOtherTeam = Guid.NewGuid();

            var interviewsInStaus = new List<InterviewCommentedStatus>();

            interviewsInStaus.Add(Create.Entity.InterviewCommentedStatus(interviewerId: user,
                supervisorId: supervisorId,
                timestamp: input.From.Date.AddHours(-1)));
            for (var i = 0; i < 20; i++)
                interviewsInStaus.Add(Create.Entity.InterviewCommentedStatus(interviewerId: Guid.NewGuid(),
                    supervisorId: supervisorId,
                    timestamp: input.From.Date.AddHours(-1)));
            interviewsInStaus.Add(Create.Entity.InterviewCommentedStatus(interviewerId: userFromOtherTeam,
                supervisorId: Guid.NewGuid(),
                timestamp: input.From.Date.AddHours(-1)));
            interviewsInStaus.Add(Create.Entity.InterviewCommentedStatus(interviewerId: user,
                supervisorId: supervisorId,
                timestamp: input.From.Date.AddDays(-2)));
            interviewsInStaus.Add(Create.Entity.InterviewCommentedStatus(interviewerId: user,
                supervisorId: supervisorId,
                timestamp: input.From.Date.AddDays(2)));

            interviewStatuses = new TestInMemoryWriter<InterviewSummary>();

            interviewStatuses.Store(
                Create.Entity.InterviewSummary(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    statuses: interviewsInStaus.ToArray()), "2");

            quantityReportFactory = CreateQuantityReportFactory(interviewStatuses);
            BecauseOf();
        }

        private void BecauseOf()
        {
            result = quantityReportFactory.Load(input);
        }

        [Test]
        public void should_return_20_rows()
        {
            result.Items.Should().HaveCount(20);
        }

        [Test]
        public void should_return_first_row_with_1_interview_at_first_period_and_zero_interviews_at_second()
        {
            result.Items.First().QuantityByPeriod.Should().Equal(1, 0);
        }

        [Test]
        public void should_return_first_row_with_1_in_Total()
        {
            result.Items.First().Total.Should().Be(1);
        }

        [Test]
        public void should_return_first_row_with_0_5_in_Average()
        {
            result.Items.First().Average.Should().Be(0.5);
        }

        [Test]
        public void should_return_total_row_with_21_interview_at_first_period_and_zero_interviews_at_second()
        {
            result.TotalRow.QuantityByPeriod.Should().Equal(0, 21);
        }

        [Test]
        public void should_return__total__row_with_21_in_Total()
        {
            result.TotalRow.Total.Should().Be(21);
        }

        [Test]
        public void should_return__total__row_with_0_5_in_Average()
        {
            result.TotalRow.Average.Should().Be(10.5);
        }
    }
}