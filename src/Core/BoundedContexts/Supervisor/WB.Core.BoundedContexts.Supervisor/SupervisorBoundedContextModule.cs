using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Implementation;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.Infrastructure;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind(typeof(ITemporaryDataStorage<>)).To(typeof(FileTemporaryDataStorage<>));
        }
    }
}
