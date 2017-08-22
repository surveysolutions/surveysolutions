using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    internal class when_load_quantity_report_for_supervisors_by_unknown_period : QuantityReportFactoryTestContext
    {
        [Test]
        public void should_throw_ArgumentException() {
            var input = CreateQuantityByInterviewersReportInputModel(supervisorId: Id.g1, period: "abr");

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();

            var quantityReportFactory = CreateQuantityReportFactory(interviewStatuses: interviewStatuses);

            Assert.Throws<ArgumentException>(() => quantityReportFactory.Load(input));
        }
    }
}
