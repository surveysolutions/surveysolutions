using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Tests.Integration.FileSystemIOAccessorTests
{
    internal class when_creating_not_existing_folder_and_it_s_parent_folders_also_do_not_exist
    {
        Establish context = () =>
        {
            fileSystemAccessor = Create.FileSystemIOAccessor();

            DeleteFolderIfExists("a/b/c", fileSystemAccessor);
            DeleteFolderIfExists("a/b", fileSystemAccessor);
            DeleteFolderIfExists("a", fileSystemAccessor);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                fileSystemAccessor.CreateDirectory("a/b/c"));

        It should_not_fail = () =>
            exception.ShouldBeNull();

        It should_create_folder_and_it_s_parent_folders_as_well = () =>
            fileSystemAccessor.IsDirectoryExists("a/b/c").ShouldBeTrue();

        Cleanup stuff = () =>
        {
            DeleteFolderIfExists("a/b/c", fileSystemAccessor);
            DeleteFolderIfExists("a/b", fileSystemAccessor);
            DeleteFolderIfExists("a", fileSystemAccessor);
        };

        private static FileSystemIOAccessor fileSystemAccessor;
        private static Exception exception;

        private static void DeleteFolderIfExists(string folder, FileSystemIOAccessor fileSystemAccessor)
        {
            if (!fileSystemAccessor.IsDirectoryExists(folder))
                return;

            fileSystemAccessor.DeleteDirectory(folder);
        }
    }
}