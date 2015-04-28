using Ninject.Modules;
using WB.Core.Infrastructure.Android.Implementation.Services.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.Infrastructure.Android
{
    public class FileSystemModule : NinjectModule
    {
        private readonly string pathToQuestionnaireAssemblies;
        public FileSystemModule(string pathToQuestionnaireAssemblies)
        {
            this.pathToQuestionnaireAssemblies = pathToQuestionnaireAssemblies;
        }

        public override void Load()
        {
            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();
            this.Bind<IQuestionnaireAssemblyFileAccessor>()
                .To<QuestionnaireAssemblyFileAccessor>().InSingletonScope()
                .WithConstructorArgument("assemblyStorageDirectory", this.pathToQuestionnaireAssemblies);
        }
    }
}