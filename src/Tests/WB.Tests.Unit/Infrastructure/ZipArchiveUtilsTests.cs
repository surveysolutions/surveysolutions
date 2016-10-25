using System.IO;
using Ionic.Zip;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.Infrastructure.FileSystem;
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
            //when
            var protectedZipStream = zipArchiveUtils.ProtectZipWithPassword(inputZipStream, password);
            //then
            var protectedZipFile = ZipFile.Read(protectedZipStream);
            var unZippedStream = new MemoryStream();
            foreach (var zipEntry in protectedZipFile)
                Catch.Only<BadPasswordException>(() => zipEntry.Extract(unZippedStream)).ShouldNotBeNull();
        }

        private static ZipArchiveUtils CreateZipArchiveUtils(IFileSystemAccessor fileSystemAccessor = null)
            => new ZipArchiveUtils(fileSystemAccessor ?? Stub<IFileSystemAccessor>.WithNotEmptyValues);
    }
}
