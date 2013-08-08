using Ninject.Modules;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireRepository>().To<QuestionnaireRepository>();
            this.Bind<IExpressionProcessor>().To<ExpressionProcessor>();
        }
    }
}