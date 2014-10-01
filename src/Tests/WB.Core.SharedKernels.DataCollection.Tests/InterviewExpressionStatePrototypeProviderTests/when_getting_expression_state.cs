using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewExpressionStatePrototypeProviderTests
{
    [Ignore("bulk test run failed on server build")]
    internal class when_getting_expression_state : InterviewExpressionStatePrototypeProviderTestContext
    {
        private Establish context = () =>
        {
            var path = typeof (IInterviewExpressionState).Assembly.Location;

            var result = new Mock<IQuestionnaireAssemblyFileAccessor>();
            result.Setup(x => x.GetFullPathToAssembly(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(path);

            var questionnareAssemblyFileAccessorMock = CreateIQuestionnareAssemblyFileAccessorMock(path);

            interviewExpressionStatePrototype = CreateInterviewExpressionStatePrototype(questionnareAssemblyFileAccessorMock.Object);
        };

        private Because of = () =>
                result = interviewExpressionStatePrototype.GetExpressionState(id, version);
/*
            RunInAnotherAppDomain(() =>
    {
        result = interviewExpressionStatePrototype.GetExpressionState(id, version);
    });
*/

        It should_provide_not_null_value = () =>
            result.ShouldNotBeNull();

        private static InterviewExpressionStatePrototypeProvider interviewExpressionStatePrototype;
        private static Guid id = Guid.Parse("33332222111100000000111122223333");
        private static long version = 3;

        private static IInterviewExpressionState result;
    }
}
