using Ninject.Modules;
using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.ExpressionProcessor
{
    public class ExpressionProcessorModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IExpressionProcessor>().To<WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services.ExpressionProcessor>().InSingletonScope();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();

            this.Bind<IVariableNameValidator>().To<VariableNameValidator>();
        }
    }
}
