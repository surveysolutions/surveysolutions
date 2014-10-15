using System;
using AppDomainToolkit;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewExpressionStatePrototypeProviderTests
{
    internal class when_getting_expression_state : InterviewExpressionStatePrototypeProviderTestContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            isResultNotNull = RemoteFunc.Invoke(appDomainContext.Domain, () =>
            {
                Guid id = Guid.Parse("33332222111100000000111122223333");
                long version = 3;

                var path = typeof (IInterviewExpressionState).Assembly.Location;

                var questionnareAssemblyFileAccessorMock = CreateIQuestionnareAssemblyFileAccessorMock(path);

                var interviewExpressionStatePrototype = CreateInterviewExpressionStatePrototype(questionnareAssemblyFileAccessorMock.Object);

                return interviewExpressionStatePrototype.GetExpressionState(id, version) != null;
            });

        It should_provide_not_null_value = () =>
            isResultNotNull.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static bool isResultNotNull;
    }
}
