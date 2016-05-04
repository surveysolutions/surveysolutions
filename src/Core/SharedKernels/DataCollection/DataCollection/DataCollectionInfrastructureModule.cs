using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionInfrastructureModule : IModule
    {
        private readonly string basePath;

        public DataCollectionInfrastructureModule(string basePath)
        {
            this.basePath = basePath;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingletonWithConstructorArgument<IPlainInterviewFileStorage, PlainInterviewFileStorage>(
                "rootDirectoryPath", this.basePath);

            registry.BindAsSingleton<IPlainQuestionnaireRepository, PlainQuestionnaireRepositoryWithCache>();
        }
    }
}