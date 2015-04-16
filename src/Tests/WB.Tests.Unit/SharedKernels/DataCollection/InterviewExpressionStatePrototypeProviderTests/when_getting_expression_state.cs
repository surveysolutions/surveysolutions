using System;
using AppDomainToolkit;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStatePrototypeProviderTests
{
    internal class when_getting_expression_state : InterviewExpressionStatePrototypeProviderTestContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            isResultNotNull = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid id = Guid.Parse("33332222111100000000111122223333");
                long version = 3;

                var path = typeof (IInterviewExpressionState).Assembly.Location;

                AssemblyContext.SetupServiceLocator();

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

        static AppDomainContext appDomainContext;
        static bool isResultNotNull;
    }
}