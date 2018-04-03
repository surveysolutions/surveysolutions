using System;
using AppDomainToolkit;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStatePrototypeProviderTests
{
    internal class when_getting_expression_state : InterviewExpressionStatePrototypeProviderTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
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

        [NUnit.Framework.Test] public void should_provide_not_null_value () =>
            isResultNotNull.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null; 
        }

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static bool isResultNotNull;
    }
}
