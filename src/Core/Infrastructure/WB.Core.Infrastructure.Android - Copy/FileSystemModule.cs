using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.FileSystem;

namespace WB.Core.Infrastructure.Android
{
    public class FileSystemModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IFileSystemService>().To<FileSystemService>().InSingletonScope();
        }
    }
}