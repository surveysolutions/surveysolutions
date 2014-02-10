using System;
using Ncqrs;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        private readonly string currentFolderPath;

        public SupervisorBoundedContextModule(string currentFolderPath)
        {
            this.currentFolderPath = currentFolderPath;
        }

        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<IDataExportService>().To<DataExportService>().WithConstructorArgument("folderPath", currentFolderPath);

            this.Unbind<IReadSideRepositoryWriter<InterviewDataExportView>>();
            this.Bind<IReadSideRepositoryWriter<InterviewDataExportView>>().To<CsvInterviewDataExportViewWriter>();

            this.Unbind<IReadSideRepositoryWriter<QuestionnaireExportStructure>>();
            this.Bind<IReadSideRepositoryWriter<QuestionnaireExportStructure>>().To<FileBaseQuestionnaireExportStructureWriter>().WithConstructorArgument("folderPath", currentFolderPath);

            this.Bind(typeof (ITemporaryDataStorage<>)).To(typeof (FileTemporaryDataStorage<>));

            Action<Guid, long> additionalEventChecker = this.AdditionalEventChecker;

            this.Bind<IReadSideRepositoryReader<InterviewData>>()
                .To<ReadSideRepositoryReaderWithSequence<InterviewData>>().InSingletonScope()
                .WithConstructorArgument("additionalEventChecker", additionalEventChecker);

            this.Bind<ITabletInformationService>().ToMethod(c => new FileBasedTabletInformationService(currentFolderPath));
        }

        protected void AdditionalEventChecker(Guid interviewId, long sequence)
        {
            Kernel.Get<IIncomePackagesRepository>().ProcessItem(interviewId, sequence);
        }
    }
}
