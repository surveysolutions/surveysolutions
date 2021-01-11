using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Directory.SetCurrentDirectory("data");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
