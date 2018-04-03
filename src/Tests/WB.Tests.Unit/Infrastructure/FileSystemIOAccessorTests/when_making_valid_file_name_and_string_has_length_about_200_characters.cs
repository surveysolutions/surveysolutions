using FluentAssertions;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_making_valid_file_name_and_string_has_length_about_200_characters
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileName = "file" + new string('A', 132);
            fileSystemIOAccessor = Create.Service.FileSystemIOAccessor();
            BecauseOf();
        }

        public void BecauseOf() =>
            newFileName = fileSystemIOAccessor.MakeValidFileName(fileName);

        [NUnit.Framework.Test] public void should_return_file_name_of_128_chars_length () =>
            newFileName.Should().Be("file" + new string('A', 124));

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string fileName;
        private static string newFileName;
    }
}
