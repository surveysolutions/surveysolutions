using System.IO;
using NUnit.Framework;

namespace WB.UI.Designer.Tests.Integration
{
    using System.IO.Compression;

    using WB.UI.Desiner.Utilities.Compression;

    [TestFixture]
    public class ZipUtilsTests
    {
        [Test]
        public void ZipDate_When_DataIsNotEmptyString_Then_FileIsCreatedWithUtf8Encodig()
        {
            // arrange
            ZipUtils target = CreateZipUtils();

            // act
            string helloworld = "helloworld";

            var file = target.Zip(helloworld);

            // assert

            using (GZipStream zip = new GZipStream(file, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(zip, System.Text.Encoding.UTF8))
                {
                    Assert.That(reader.ReadToEnd(), Is.EqualTo(helloworld));
                }
               
            }

        }

        private ZipUtils CreateZipUtils()
        {
            return new ZipUtils();
        }
    }
}
