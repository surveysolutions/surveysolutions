using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Export;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Implementation;
using WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<IDataExportService>().To<DataExportService>();
            this.Bind<IEnvironmentSupplier<InterviewDataExportView>>().To<StataEnvironmentSupplier>();
            this.Bind<IExportProvider<InterviewDataExportView>>().To<IterviewExporter>();
            this.Bind(typeof(ITemporaryDataStorage<>)).To(typeof(FileTemporaryDataStorage<>));

            
            this.Bind<IReadSideRepositoryWriter<InterviewData>, IReadSideRepositoryReader<InterviewData>>()
                .To<InterviewDataRepositoryWriterWithCache>()
                .InSingletonScope();
        }
    }
}
