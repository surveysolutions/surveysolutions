using FluentAssertions;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_file_name_is_null
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            fileSystemIOAccessor = Create.Service.FileSystemIOAccessor();
            BecauseOf();
        }

        public void BecauseOf() =>
            newFileName = fileSystemIOAccessor.MakeStataCompatibleFileName(null);

        [NUnit.Framework.Test]
        public void should_return_string_with_underscore() =>
            newFileName.Should().Be("_");

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string newFileName;
    }
}
