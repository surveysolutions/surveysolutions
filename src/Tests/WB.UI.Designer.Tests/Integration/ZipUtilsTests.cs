using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using NUnit.Framework;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Tests.Integration
{
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

            var file = target.ZipDate(helloworld);

            // assert

            using (ZipFile zip = ZipFile.Read(new MemoryStream(file)))
            {
                foreach (ZipEntry e in zip)
                {
                    Assert.That(e.AlternateEncoding, Is.EqualTo(Encoding.UTF8));
                }
            }

        }

        private ZipUtils CreateZipUtils()
        {
            return new ZipUtils();
        }
    }
}
