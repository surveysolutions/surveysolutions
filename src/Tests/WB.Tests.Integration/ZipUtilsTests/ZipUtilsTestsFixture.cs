using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Tests.Integration.ZipUtilsTests
{
    [TestFixture]
    public class ZipUtilsTestsFixture
    {
        [Test]
        public void ZipDate_When_DataIsNotEmptyString_Then_FileIsCreatedWithUtf8Encodig()
        {
            // arrange
            GZipJsonCompressor target = this.CreateZipUtils();

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

        private GZipJsonCompressor CreateZipUtils()
        {
            return new GZipJsonCompressor(new NewtonJsonUtils());
        }
    }
}
