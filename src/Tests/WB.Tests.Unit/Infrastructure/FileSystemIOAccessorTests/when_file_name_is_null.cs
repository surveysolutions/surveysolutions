using Machine.Specifications;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_file_name_is_null
    {
        Establish context = () =>
        {
            fileSystemIOAccessor = Create.Service.FileSystemIOAccessor();
        };

        Because of = () =>
            newFileName = fileSystemIOAccessor.MakeStataCompatibleFileName(null);

        It should_return_string_with_underscore = () =>
            newFileName.ShouldEqual("_");

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string newFileName;
    }
}
