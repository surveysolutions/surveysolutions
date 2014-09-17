using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests
{
    [Subject(typeof(PlainFileRepository))]
    class PlainFileRepositoryTestContext
    {
        protected static PlainFileRepository CreatePlainFileRepository()
        {
            Cleanup();
            return new PlainFileRepository(new FileSystemIOAccessor(), GetBasePath());
        }

        protected static string GetBasePath()
        {
            var pathToDll = Assembly.GetExecutingAssembly().Location;
            var directoryPath = Path.GetDirectoryName(pathToDll);
            return Path.Combine(directoryPath, "Test");
        }

        protected static void Cleanup()
        {
            var directoryPath = GetBasePath();
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath,true);
        }
    }
}
