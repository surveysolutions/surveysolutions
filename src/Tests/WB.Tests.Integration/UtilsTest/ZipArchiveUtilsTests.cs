using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Integration.UtilsTest
{
    [TestOf(typeof(ZipArchiveUtils))]
    [TestFixture]
    internal class ZipArchiveUtilsTests
    {
        [OneTimeSetUp]
        public void SetUp() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        [Test]
        public void should_preserve_zero_length_files_in_archive()
        {
            const string file_path = "test.zip";

            try
            {
                using (var fs = File.Create(file_path))
                {
                    using (var arch = new CompressionZipArchive(fs))
                    {
                        arch.CreateEntry("non_empty.file", Encoding.UTF8.GetBytes("test"));
                        arch.CreateEntry("empty.file", Array.Empty<byte>());
                    }
                }
                
                using (var zip = ZipFile.OpenRead(file_path))
                {
                    var nonEmpty = zip.Entries.SingleOrDefault(e => e.Name == "non_empty.file");
                    Assert.That(nonEmpty, Is.Not.Null);

                    var emptyFile = zip.Entries.SingleOrDefault(e => e.Name == "empty.file");
                    Assert.That(nonEmpty, Is.Not.Null);

                    using var streamEmpty = emptyFile.Open();
                    using (var ms = new MemoryStream())
                    {
                        streamEmpty.CopyTo(ms);
                        Assert.That(ms.ToArray(), Has.Length.EqualTo(0));
                    }
                    
                    using var stream = nonEmpty.Open();
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        Assert.That(ms.ToArray(), Has.Length.EqualTo(4));
                    }
                }
            }
            finally
            {
                File.Delete("test.zip");
            }
        }

        private static ZipArchiveUtils CreateZipArchiveUtils() => new ZipArchiveUtils();
    }
}
