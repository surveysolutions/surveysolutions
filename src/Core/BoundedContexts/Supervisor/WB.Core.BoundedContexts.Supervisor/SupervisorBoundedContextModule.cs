using System;
using Ncqrs;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

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
            this.Bind(typeof (ITemporaryDataStorage<>)).To(typeof (FileTemporaryDataStorage<>));

            this.Bind<IFunctionalDenormalizer>().To<InterviewSummaryDenormalizerFunctional>();
            this.Bind<IFunctionalDenormalizer>().To<InterviewDenormalizerFunctional>();

            Action<Guid, long> additionalEventChecker = this.AdditionalEventChecker;

            this.Bind<IReadSideRepositoryReader<InterviewData>>()
                .To<ReadSideRepositoryReaderWithSequence<InterviewData>>().InSingletonScope()
                .WithConstructorArgument("additionalEventChecker", additionalEventChecker);
        }

        protected void AdditionalEventChecker(Guid interviewId, long sequence)
        {
            Kernel.Get<IIncomePackagesRepository>().ProcessItem(interviewId, sequence);
        }
    }
}
