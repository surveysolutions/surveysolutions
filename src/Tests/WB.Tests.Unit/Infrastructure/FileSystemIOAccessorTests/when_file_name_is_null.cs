using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Tests.Unit.Infrastructure.FileSystemIOAccessorTests
{
    internal class when_file_name_is_null
    {
        Establish context = () =>
        {
            fileSystemIOAccessor = Create.FileSystemIOAccessor();
        };

        Because of = () =>
            newFileName = fileSystemIOAccessor.MakeValidFileName(null);

        It should_return_empty_string = () =>
            newFileName.ShouldEqual("");

        private static FileSystemIOAccessor fileSystemIOAccessor;
        private static string newFileName;
    }
}
