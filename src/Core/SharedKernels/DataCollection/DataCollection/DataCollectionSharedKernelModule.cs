using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.Accessors;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
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
        private readonly string syncDirectoryName;
        private readonly string dataDirectoryName;
        private readonly string questionnaireAssembliesDirectoryName;

        public DataCollectionSharedKernelModule(bool usePlainQuestionnaireRepository, string basePath, string syncDirectoryName = "SYNC",
            string dataDirectoryName = "InterviewData", string questionnaireAssembliesFolder = "QuestionnaireAssemblies")
        {
            this.usePlainQuestionnaireRepository = usePlainQuestionnaireRepository;
            this.basePath = basePath;
            this.syncDirectoryName = syncDirectoryName;
            this.dataDirectoryName = dataDirectoryName;
            this.questionnaireAssembliesDirectoryName = questionnaireAssembliesFolder;
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

            this.Bind<IPlainInterviewFileStorage>()
              .To<PlainInterviewFileStorage>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.basePath).WithConstructorArgument("dataDirectoryName", this.dataDirectoryName);

            this.Bind<IInterviewSynchronizationFileStorage>()
                .To<InterviewSynchronizationFileStorage>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.basePath).WithConstructorArgument("syncDirectoryName", this.syncDirectoryName);

            this.Bind<IQuestionnaireAssemblyFileAccessor>()
                .To<QuestionnaireAssemblyFileAccessor>().InSingletonScope().WithConstructorArgument("folderPath", basePath).WithConstructorArgument("assemblyDirectoryName", questionnaireAssembliesDirectoryName);

            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();
        }
    }
}