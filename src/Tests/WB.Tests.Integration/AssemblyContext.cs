using Machine.Specifications;

namespace WB.Tests.Integration
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            Setup.SetupMockedServiceLocator();
        }

        public void OnAssemblyComplete() {}
    }
}
