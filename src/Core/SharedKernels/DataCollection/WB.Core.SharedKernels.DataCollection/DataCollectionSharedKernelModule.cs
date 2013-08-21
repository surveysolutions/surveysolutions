using Ninject.Modules;
using WB.Core.SharedKernels.DataCollection.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireRepository>().To<QuestionnaireRepository>();
            this.Bind<IExpressionProcessor>().To<ExpressionProcessor>();
            this.Bind(typeof(IVersionedReadSideRepositoryWriter<>)).To(typeof(VersionedReadSideRepositoryWriter<>));
            this.Bind(typeof(IVersionedReadSideRepositoryReader<>)).To(typeof(VersionedReadSideRepositoryReader<>));
        }
    }
}