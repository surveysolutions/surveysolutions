using System.IO;
using NUnit.Framework;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.UI.Designer.Tests.Integration
{
    using System.IO.Compression;

    using WB.Core.SharedKernel.Utils.Compression;

    [TestFixture]
    public class ZipUtilsTests
    {
        [Test]
        public void ZipDate_When_DataIsNotEmptyString_Then_FileIsCreatedWithUtf8Encodig()
        {
            // arrange
            IStringCompressor target = CreateZipUtils();

            // act
            string helloworld = "helloworld";

            var file = target.Compress(helloworld);

            // assert

            using (GZipStream zip = new GZipStream(file, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(zip, System.Text.Encoding.UTF8))
                {
                    Assert.That(reader.ReadToEnd(), Is.EqualTo(helloworld));
                }
               
            }

        }

        private IStringCompressor CreateZipUtils()
        {
            return new GZipJsonCompressor(new NewtonJsonUtils());
        }
    }
}
