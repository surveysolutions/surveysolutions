using Ninject.Modules;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Tester.Ninject
{
    public class DataCollectionModule: NinjectModule
    {
        private readonly string basePath;
        private readonly string dataDirectoryName;

        public DataCollectionModule(string basePath, string dataDirectoryName = "InterviewData")
        {
            this.basePath = basePath;
            this.dataDirectoryName = dataDirectoryName;
        }

        public override void Load()
        {
            this.Bind<IInterviewExpressionStateUpgrader>().To<InterviewExpressionStateUpgrader>().InSingletonScope();
            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();
            this.Bind<IPlainInterviewFileStorage>()
                .To<PlainInterviewFileStorage>().InSingletonScope()
                                                .WithConstructorArgument("rootDirectoryPath", this.basePath)
                                                .WithConstructorArgument("dataDirectoryName", this.dataDirectoryName);
        }
    }
}