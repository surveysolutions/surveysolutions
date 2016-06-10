using Machine.Specifications;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_making_valid_file_name_and_string_has_length_about_500_characters
    {
        Establish context = () =>
        {
            fileName = "file" + new string('A', 500);
            fileSystemIOAccessor = Create.Service.FileSystemIOAccessor();
        };

        Because of = () =>
            newFileName = fileSystemIOAccessor.MakeValidFileName(fileName);

        It should_return_file_name_of_500_length = () =>
            newFileName.ShouldEqual("file" + new string('A', 496));

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string fileName;
        private static string newFileName;
    }
}