using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : NinjectModule
    {
        private readonly bool usePlainQuestionnaireRepository;
        private readonly string basePath;

        public DataCollectionSharedKernelModule(bool usePlainQuestionnaireRepository, string basePath)
        {
            this.usePlainQuestionnaireRepository = usePlainQuestionnaireRepository;
            this.basePath = basePath;
        }

        public override void Load()
        {
            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepository>();

            if (this.usePlainQuestionnaireRepository)
            {
                this.Bind<IQuestionnaireRepository>().To<PlainQuestionnaireRepository>();
            }
            else
            {
                this.Bind<IQuestionnaireRepository>().To<DomainQuestionnaireRepository>().InSingletonScope(); // has internal cache, so should be singleton
            }

            this.Bind<IQuestionnaireFactory>().To<QuestionnaireFactory>();

            this.Bind(typeof(IVersionedReadSideRepositoryWriter<>)).To(typeof(VersionedReadSideRepositoryWriter<>));
            this.Bind(typeof(IVersionedReadSideRepositoryReader<>)).To(typeof(VersionedReadSideRepositoryReader<>));

            this.Bind<IQuestionnaireRosterStructureFactory>().To<QuestionnaireRosterStructureFactory>();

            this.Bind<IPlainFileRepository>()
              .To<PlainFileRepository>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.basePath);

            this.Bind<IFileSyncRepository>()
            .To<FileSyncRepository>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.basePath);
        }
    }
}