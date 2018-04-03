using System.IO;
using FluentAssertions;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_file_name_contains_invalid_characters
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            fileName = "nastya" + new string(Path.GetInvalidFileNameChars());
            fileSystemIOAccessor = Create.Service.FileSystemIOAccessor();
            BecauseOf();
        }

        public void BecauseOf() =>
            newFileName = fileSystemIOAccessor.MakeStataCompatibleFileName(fileName);

        [NUnit.Framework.Test]
        public void should_from_file_name_be_cut_all_invalid_characters() =>
            newFileName.Should().Be("nastya_");

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string fileName;
        private static string newFileName;
    }
}
