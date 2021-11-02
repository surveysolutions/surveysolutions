using FluentAssertions;
using NUnit.Framework;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Integration.FileSystemIOAccessorTests
{
    internal class when_creating_not_existing_folder_and_it_s_parent_folders_also_do_not_exist
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = new FileSystemIOAccessor();

            DeleteFolderIfExists("a/b/c", fileSystemAccessor);
            DeleteFolderIfExists("a/b", fileSystemAccessor);
            DeleteFolderIfExists("a", fileSystemAccessor);

            fileSystemAccessor.CreateDirectory("a/b/c");
        }

        [NUnit.Framework.Test] public void should_create_folder_and_it_s_parent_folders_as_well () =>
            fileSystemAccessor.IsDirectoryExists("a/b/c").Should().BeTrue();


        [OneTimeTearDown]
        public void  stuff()
        {
            DeleteFolderIfExists("a/b/c", fileSystemAccessor);
            DeleteFolderIfExists("a/b", fileSystemAccessor);
            DeleteFolderIfExists("a", fileSystemAccessor);
        }

        private static FileSystemIOAccessor fileSystemAccessor;

        private static void DeleteFolderIfExists(string folder, FileSystemIOAccessor fileSystemAccessor)
        {
            if (!fileSystemAccessor.IsDirectoryExists(folder))
                return;

            fileSystemAccessor.DeleteDirectory(folder);
        }
    }
}
