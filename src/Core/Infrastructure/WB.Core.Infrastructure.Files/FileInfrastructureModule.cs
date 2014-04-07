using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
