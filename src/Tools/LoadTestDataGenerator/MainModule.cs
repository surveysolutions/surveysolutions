using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.ServiceModel;
using Ninject.Modules;
using WB.Core.SharedKernel.Utils.Compression;

namespace LoadTestDataGenerator
{
    public class MainModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICommandService>().ToConstant(Ncqrs.NcqrsEnvironment.Get<ICommandService>());
            this.Bind<IStringCompressor>().ToConstant(new GZipJsonCompressor()).InSingletonScope();
        }
    }
}
