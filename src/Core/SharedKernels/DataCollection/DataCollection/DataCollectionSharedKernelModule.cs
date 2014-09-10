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

        public DataCollectionSharedKernelModule(bool usePlainQuestionnaireRepository)
        {
            this.usePlainQuestionnaireRepository = usePlainQuestionnaireRepository;
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

            this.Bind<IInterviewExpressionStateProvider>().To<InterviewExpressionStateProvider>();
            //this.Bind<IExpressionProcessor>().To<ExpressionProcessor>();

            this.Bind<IQuestionnaireFactory>().To<QuestionnaireFactory>();

            this.Bind(typeof(IVersionedReadSideRepositoryWriter<>)).To(typeof(VersionedReadSideRepositoryWriter<>));
            this.Bind(typeof(IVersionedReadSideRepositoryReader<>)).To(typeof(VersionedReadSideRepositoryReader<>));

            this.Bind<IQuestionnaireRosterStructureFactory>().To<QuestionnaireRosterStructureFactory>();
        }
    }
}