using System.Text;
using NUnit.Framework;

namespace WB.Tests.Web
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [OneTimeSetUp]
        public void SetupAssembly()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // https://github.com/JanKallman/EPPlus/issues/31
        }
    }
}
