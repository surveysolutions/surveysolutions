using Ninject.Modules;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.Infrastructure.Files
{
    public class FileInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IFileSystemAccessor>().To<FileSystemIOAccessor>();
            this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();
        }
    }
}
