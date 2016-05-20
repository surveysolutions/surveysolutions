﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    internal class when_load_overall_speed_report_for_interviewers_by_week_period : SpeedReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateSpeedBetweenStatusesByInterviewersReportInputModel(supervisorId: supervisorId, period: "w");

            var user = Create.Other.UserDocument(supervisorId: supervisorId);

            interviewStatusTimeSpans = new TestInMemoryWriter<InterviewStatusTimeSpans>();
            interviewStatusTimeSpans.Store(
                Create.Other.InterviewStatusTimeSpans(questionnaireId: input.QuestionnaireId,
                    questionnaireVersion: input.QuestionnaireVersion,
                    timeSpans: new[]
                    {
                        Create.Other.TimeSpanBetweenStatuses(interviewerId: user.PublicKey,
                            supervisorId:supervisorId,
                            timestamp: input.From.Date.AddHours(1), 
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(-35)),
                          Create.Other.TimeSpanBetweenStatuses(interviewerId: Guid.NewGuid(),
                            supervisorId:Guid.NewGuid(),
                            timestamp: input.From.Date.AddHours(1),
                            timeSpanWithPreviousStatus: TimeSpan.FromMinutes(-35))
                    }), "2");

            quantityReportFactory = CreateSpeedReportFactory(interviewStatusTimeSpans: interviewStatusTimeSpans);
        };

        Because of = () =>
            result = quantityReportFactory.Load(input);

        It should_return_one_row = () =>
            result.Items.Count().ShouldEqual(1);

        It should_return_first_row_with_positive_35_minutes_per_interview_at_first_period_and_null_minutes_per_interview_at_second = () =>
            result.Items.First().SpeedByPeriod.ShouldEqual(new double?[] { 35, null });

        It should_return_first_row_with_positive_35_minutes_in_Total = () =>
            result.Items.First().Total.ShouldEqual(35);

        It should_return_first_row_with_positive_35_minutesin_Average = () =>
           result.Items.First().Average.ShouldEqual(35);

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedBetweenStatusesByInterviewersReportInputModel input;
        private static SpeedByResponsibleReportView result;
        private static TestInMemoryWriter<InterviewStatusTimeSpans> interviewStatusTimeSpans;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
