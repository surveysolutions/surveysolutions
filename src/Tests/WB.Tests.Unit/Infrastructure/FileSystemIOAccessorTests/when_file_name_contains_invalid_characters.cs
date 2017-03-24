using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_file_name_contains_invalid_characters
    {
        Establish context = () =>
        {
            fileName = "nastya" + new string(Path.GetInvalidFileNameChars());
            fileSystemIOAccessor = Create.Service.FileSystemIOAccessor();
        };

        Because of = () =>
            newFileName=fileSystemIOAccessor.MakeStataCompatibleFileName(fileName);

        It should_from_file_name_be_cut_all_invalid_characters = () =>
            newFileName.ShouldEqual("nastya_");

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string fileName;
        private static string newFileName;
    }
}
