using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.ExpressionProcessor
{
    public class ExpressionProcessorModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IExpressionProcessor>().To<WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services.ExpressionProcessor>().InSingletonScope();
        }
    }
}
