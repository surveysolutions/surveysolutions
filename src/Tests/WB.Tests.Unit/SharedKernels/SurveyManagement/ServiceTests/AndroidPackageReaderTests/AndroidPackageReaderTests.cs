using System.IO;
using System.Reflection;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AndroidPackageReaderTests
{
    internal class AndroidPackageReaderTests
    {
        [Test]
        public void when_reading_package_info()
        {
            var reader = new AndroidPackageReader();

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.AndroidPackageReaderTests.TestManifest._apk";

            var manifestResourceStream = assembly.GetManifestResourceStream(resourceName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);

                var version = reader.Read(memoryStream);
                Assert.That(version.BuildNumber, Is.EqualTo(25987));
            }
        }
    }
}
