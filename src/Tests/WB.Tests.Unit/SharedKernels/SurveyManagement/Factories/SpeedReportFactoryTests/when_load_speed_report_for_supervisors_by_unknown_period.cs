using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    internal class when_load_speed_report_for_supervisors_by_unknown_period : SpeedReportFactoryTestContext
    {
        Establish context = () =>
        {
            input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, period: "abr");

            interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();

            quantityReportFactory = CreateSpeedReportFactory(interviewStatuses: interviewStatuses);
        };

        Because of = () =>
            exception = Catch.Only<ArgumentException>(() =>
                quantityReportFactory.Load(input));

        It should_throw_ArgumentException = () =>
            exception.ShouldNotBeNull();

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedByInterviewersReportInputModel input;
        private static ArgumentException exception;
        private static TestInMemoryWriter<InterviewStatuses> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
