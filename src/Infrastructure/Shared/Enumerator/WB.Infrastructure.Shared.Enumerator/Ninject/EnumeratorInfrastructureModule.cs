using Ninject.Modules;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Infrastructure.Shared.Enumerator.Ninject
{
    public class EnumeratorInfrastructureModule : NinjectModule
    {
        private const string dataDirectoryName = "InterviewData";

        private readonly string basePath;

        public EnumeratorInfrastructureModule(string basePath)
        {
            this.basePath = basePath;
        }

        public override void Load()
        {
            this.Bind<IPlainInterviewFileStorage>().To<PlainInterviewFileStorage>().InSingletonScope()
                .WithConstructorArgument("rootDirectoryPath", this.basePath)
                .WithConstructorArgument("dataDirectoryName", dataDirectoryName);
        }
    }
}