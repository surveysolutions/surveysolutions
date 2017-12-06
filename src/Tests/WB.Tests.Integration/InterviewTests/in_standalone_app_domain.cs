using AppDomainToolkit;
using Machine.Specifications;

namespace WB.Tests.Integration.InterviewTests
{
    public class in_standalone_app_domain : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
    }
}