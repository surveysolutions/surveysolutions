using System;
using Ncqrs;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        private readonly string currentFolderPath;
        private readonly int supportedQuestionnaireVersionMajor;
        private readonly int supportedQuestionnaireVersionMinor;
        private readonly int supportedQuestionnaireVersionPatch;

        public SupervisorBoundedContextModule(string currentFolderPath, int supportedQuestionnaireVersionMajor, int supportedQuestionnaireVersionMinor, int supportedQuestionnaireVersionPatch)
        {
            this.currentFolderPath = currentFolderPath;
            this.supportedQuestionnaireVersionMajor = supportedQuestionnaireVersionMajor;
            this.supportedQuestionnaireVersionMinor = supportedQuestionnaireVersionMinor;
            this.supportedQuestionnaireVersionPatch = supportedQuestionnaireVersionPatch;
        }

        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<IDataExportService>().To<DataExportService>().WithConstructorArgument("folderPath", currentFolderPath);

            this.Bind<ApplicationVersionSettings>().ToMethod(context => new ApplicationVersionSettings
            {
                SupportedQuestionnaireVersionMajor = this.supportedQuestionnaireVersionMajor,
                SupportedQuestionnaireVersionMinor = this.supportedQuestionnaireVersionMinor,
                SupportedQuestionnaireVersionPatch = this.supportedQuestionnaireVersionPatch
            });

            this.Unbind<ISupportedVersionProvider>();
            this.Bind<ISupportedVersionProvider>().To<SupportedVersionProvider>().InSingletonScope();

            this.Unbind<IReadSideRepositoryWriter<InterviewDataExportView>>();
            this.Bind<IReadSideRepositoryWriter<InterviewDataExportView>>().To<CsvInterviewDataExportViewWriter>();

            this.Unbind<IReadSideRepositoryWriter<QuestionnaireExportStructure>>();
            this.Bind<IReadSideRepositoryWriter<QuestionnaireExportStructure>>().To<FileBaseQuestionnaireExportStructureWriter>().WithConstructorArgument("folderPath", currentFolderPath);

            this.Bind(typeof (ITemporaryDataStorage<>)).To(typeof (FileTemporaryDataStorage<>));

            Action<Guid> additionalEventChecker = this.AdditionalEventChecker;

            this.Bind<IReadSideRepositoryReader<InterviewData>>()
                .To<ReadSideRepositoryReaderWithSequence<InterviewData>>().InSingletonScope()
                .WithConstructorArgument("additionalEventChecker", additionalEventChecker);
        }

        protected void AdditionalEventChecker(Guid interviewId)
        {
            Kernel.Get<IIncomePackagesRepository>().ProcessItem(interviewId);
        }
    }
}
