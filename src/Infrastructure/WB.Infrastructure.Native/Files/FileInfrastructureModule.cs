using Ninject.Modules;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Infrastructure.Native.Files
{
    public class FileInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IFileSystemAccessor>().To<FileSystemIOAccessor>();
            //this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();
        }
    }
}
