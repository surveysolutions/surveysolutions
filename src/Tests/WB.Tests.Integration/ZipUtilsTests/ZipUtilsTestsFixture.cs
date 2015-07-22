using System.IO;
using System.IO.Compression;
using NUnit.Framework;

using WB.Core.GenericSubdomains.Native;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Tests.Integration.ZipUtilsTests
{
    [TestFixture]
    public class ZipUtilsTestsFixture
    {
        [Test]
        public void ZipDate_When_DataIsNotEmptyString_Then_FileIsCreatedWithUtf8Encodig()
        {
            // arrange
            JsonCompressor target = this.CreateZipUtils();

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

        private JsonCompressor CreateZipUtils()
        {
            return new JsonCompressor(new NewtonJsonUtils());
        }
    }
}
