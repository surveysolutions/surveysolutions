using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [TestFixture]
    internal class when_load_speed_report_for_supervisors_by_unknown_period : SpeedReportFactoryTestContext
    {
        [SetUp]
        public void Establish()
        {
            input = CreateSpeedByInterviewersReportInputModel(supervisorId: supervisorId, period: "abr");

            interviewStatuses = new TestInMemoryWriter<InterviewSummary>();

            quantityReportFactory = CreateSpeedReportFactory(interviewStatuses: interviewStatuses);
        }

        [Test]
        public void should_throw_ArgumentException()
        {
            exception = Catch.Only<ArgumentException>(() => quantityReportFactory.Load(input));
            exception.ShouldNotBeNull();
        }

        private static SpeedReportFactory quantityReportFactory;
        private static SpeedByInterviewersReportInputModel input;
        private static ArgumentException exception;
        private static TestInMemoryWriter<InterviewSummary> interviewStatuses;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
    }
}
