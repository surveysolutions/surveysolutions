﻿using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.Denormalizers;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        private readonly bool supervisorFunctionsEnabled;
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly DataExportSettings dataExportSettings;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        public HeadquartersBoundedContextModule(bool supervisorFunctionsEnabled, UserPreloadingSettings userPreloadingSettings, DataExportSettings dataExportSettings, InterviewDataExportSettings interviewDataExportSettings)
        {
            this.supervisorFunctionsEnabled = supervisorFunctionsEnabled;
            this.userPreloadingSettings = userPreloadingSettings;
            this.dataExportSettings = dataExportSettings;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        public override void Load()
        {
            if (!supervisorFunctionsEnabled)
            {
                this.Bind<IVersionedQuestionnaireReader>().To<VersionedQustionnaireDocumentViewFactory>();
                this.Kernel.RegisterDenormalizer<InterviewsFeedDenormalizer>();
                this.Kernel.RegisterDenormalizer<VersionedQustionnaireDocumentDenormalizer>();
                this.Kernel.RegisterDenormalizer<UsersFeedDenormalizer>();
                this.Kernel.RegisterDenormalizer<QuestionnaireFeedDenormalizer>();
            }

            CommandRegistry.Configure<User, CreateUserCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator, CreateUserCommand>());
            CommandRegistry.Configure<User, UnarchiveUserCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator, UnarchiveUserCommand>());
            CommandRegistry.Configure<User, UnarchiveUserAndUpdateCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator, UnarchiveUserAndUpdateCommand>());

            this.Bind<UserPreloadingSettings>().ToConstant(this.userPreloadingSettings);
            this.Bind<IUserBatchCreator>().To<UserBatchCreator>();

            this.Bind<IUserPreloadingVerifier>().To<UserPreloadingVerifier>().InSingletonScope();
            this.Bind<IUserPreloadingCleaner>().To<UserPreloadingCleaner>().InSingletonScope();


            this.Bind<InterviewDataExportSettings>().ToConstant(this.interviewDataExportSettings);
            this.Bind<DataExportSettings>().ToConstant(this.dataExportSettings);
            this.Bind<IFilebasedExportedDataAccessor>().To<FilebasedExportedDataAccessor>();
            this.Bind<IMetadataExportService>().To<MetadataExportService>();
            this.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            this.Bind<IDataExportProcessesService>().To<DataExportProcessesService>().InSingletonScope();
            this.Bind<IDataExporter>().To<DataExporter>().InSingletonScope();

            this.Bind<ITabularDataToExternalStatPackageExportService>().To<TabularDataToExternalStatPackageExportService>();
            this.Bind<ITabFileReader>().To<TabFileReader>();
            this.Bind<IDatasetWriterFactory>().To<DatasetWriterFactory>();
            this.Bind<IDataQueryFactory>().To<DataQueryFactory>();

            this.Bind<IEnvironmentContentService>().To<StataEnvironmentContentService>();
            this.Bind<IParaDataAccessor>().To<TabularParaDataAccessor>();

            this.Bind<TabularFormatDataExportHandler>().ToSelf();
            this.Bind<TabularFormatParaDataExportProcessHandler>().ToSelf();
            this.Bind<StataFormatExportHandler>().ToSelf();
            this.Bind<SpssFormatExportHandler>().ToSelf();
            this.Bind<BinaryFormatDataExportHandler>().ToSelf();

            this.Bind<ITabularFormatExportService>().To<ReadSideToTabularFormatExportService>();
            this.Bind<ICsvWriterService>().To<CsvWriterService>();
            this.Bind<ICsvWriter>().To<CsvWriter>();
            this.Bind<IExportViewFactory>().To<ExportViewFactory>();
            this.Bind<IDataExportStatusReader>().To<DataExportStatusReader>();
            this.Kernel.RegisterDenormalizer<QuestionnaireExportStructureDenormalizer>();
        }
    }
}
