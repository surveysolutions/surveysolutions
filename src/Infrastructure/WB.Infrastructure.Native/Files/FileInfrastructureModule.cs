using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Infrastructure.Native.Files
{
    public class FileInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IFileSystemAccessor, FileSystemIOAccessor>();
            //this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();
        }
    }
}
