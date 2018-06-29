using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Ionic.Zip;
using Ionic.Zlib;
using NUnit.Framework;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Unit.Infrastructure
{
    [TestOf(typeof(ZipArchiveUtils))]
    [TestFixture]
    internal class ZipArchiveUtilsTests
    {
        [Test]
        public void When_ProtectZipWithPassword_Then_each_file_in_zip_should_not_be_extracted_without_password()
        {
            //given
            string password = "password";
            MemoryStream inputZipStream = new MemoryStream();
            ZipFile zipFile = new ZipFile();
            zipFile.AddEntry("password_protected_file_1", "content of password protected file 1");
            zipFile.AddEntry("password_protected_file_2", "content of password protected file 2");
            zipFile.Save(inputZipStream);
            inputZipStream.Position = 0;

            var zipArchiveUtils = CreateZipArchiveUtils();
            MemoryStream protectedZipStream = new MemoryStream();
            //when
            zipArchiveUtils.ProtectZipWithPassword(inputZipStream, protectedZipStream, password);
            //then
            var protectedZipFile = ZipFile.Read(protectedZipStream);
            var unZippedStream = new MemoryStream();
            foreach (var zipEntry in protectedZipFile)
                Assert.Throws<BadPasswordException>(() => zipEntry.Extract(unZippedStream));
        }

        [TestCase("")]
        [TestCase("with password")]
        public void should_preserve_zero_length_files_in_archive(string password)
        {
            const string file_path = "test.zip";

            try
            {
                using (var fs = File.Create(file_path))
                {
                    using (var arch = new IonicZipArchive(fs, password, CompressionLevel.BestCompression))
                    {
                        arch.CreateEntry("non_empty.file", Encoding.UTF8.GetBytes("test"));
                        arch.CreateEntry("empty.file", Array.Empty<byte>());
                    }
                }

                using (var file = File.OpenRead(file_path))
                {
                    using (var zip = ZipFile.Read(file))
                    {
                        var nonEmpty = zip.Entries.SingleOrDefault(e => e.FileName == "non_empty.file");
                        Assert.That(nonEmpty, Is.Not.Null);

                        var emptyFile = zip.Entries.SingleOrDefault(e => e.FileName == "empty.file");
                        Assert.That(nonEmpty, Is.Not.Null);

                        using (var ms = new MemoryStream())
                        {
                            if (string.IsNullOrWhiteSpace(password))
                            {
                                emptyFile.ExtractWithPassword(ms, password);
                            }
                            else
                            {
                                emptyFile.Extract(ms);
                            }

                            Assert.That(ms.ToArray(), Has.Length.EqualTo(0));
                        }
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
