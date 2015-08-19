using Ninject.Modules;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.Infrastructure.Files.Android
{
    public class FileInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();
        }
    }
}