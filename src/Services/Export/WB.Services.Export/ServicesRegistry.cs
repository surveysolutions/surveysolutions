using System;
using System.IO;
using System.Runtime.CompilerServices;
using ddidotnet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.CsvExport;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Ddi;
using WB.Services.Export.Ddi.Implementation;
using WB.Services.Export.Events;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.ExportProcessHandlers.Externals;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Infrastructure.Implementation;
using WB.Services.Export.Interview;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.InterviewDataStorage.Services;
using WB.Services.Export.Jobs;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Questionnaire.Services.Implementation;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Implementation;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Export.User;
using WB.Services.Infrastructure.EventSourcing;
using WB.Services.Infrastructure.FileSystem;

[assembly: InternalsVisibleTo("WB.Services.Export.Tests")]

namespace WB.Services.Export
{
    public static class ServicesRegistry
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();

            // Transients
            services.AddTransient<IFileSystemAccessor, FileSystemAccessor>();
            services.AddTransient<IEventsHandler, EventsHandler>();
            services.AddTransient<IEventsFilter, QuestionnaireEventFilter>();
            services.AddTransient<IInterviewsToExportSource, InterviewsToExportSource>();
            services.AddTransient<IQuestionnaireStorageCache, QuestionnaireStorageCache>();
            services.AddTransient<IQuestionnaireSchemaGenerator, QuestionnaireSchemaGenerator>();
            services.AddTransient<ICsvWriter, CsvWriter>();
            services.AddTransient<ITabularFormatExportService, CsvExport.Implementation.TabularFormatExportService>();
            services.AddTransient<IProductVersion, ProductVersion>();
            services.AddTransient<ICommentsExporter, CommentsExporter>();
            services.AddTransient<IQuestionnaireExportStructureFactory, QuestionnaireExportStructureFactory>();
            services.AddTransient<IDiagnosticsExporter, DiagnosticsExporter>();
            services.AddTransient<IQuestionnaireStorage, QuestionnaireStorage>();
            services.AddTransient<IAssignmentActionsExporter, AssignmentActionsExporter>();
            services.AddTransient<IInterviewActionsExporter, InterviewActionsExporter>();
            services.AddTransient<IInterviewsExporter, InterviewsExporter>();
            services.AddTransient<IInterviewsDoFilesExporter, InterviewsDoFilesExporter>();
            services.AddTransient<IInterviewFactory, InterviewFactory>();
            services.AddTransient<IInterviewErrorsExporter, InterviewErrorsExporter>();
            services.AddTransient<IExportQuestionService, ExportQuestionService>();
            services.AddTransient<IEnvironmentContentService, StataEnvironmentContentService>();
            services.AddTransient<IFileBasedExportedDataAccessor, FileBasedExportedDataAccessor>();
            services.AddTransient<IDataExportFileAccessor, DataExportFileAccessor>();
            services.AddTransient<IQuestionnaireLabelFactory, QuestionnaireLabelFactory>();
            services.AddTransient<IExportFileNameService, ExportExportFileNameService>();
            services.AddTransient<IArchiveUtils, ZipArchiveUtils>();
            
            services.AddTransient<ITabularDataToExternalStatPackageExportService, TabularDataToExternalStatPackageExportService>();
            services.AddTransient<ITabFileReader, TabFileReader>();
            services.AddTransient<IDatasetWriterFactory, DatasetWriterFactory>();
            services.AddTransient<IDataQueryFactory, DataQueryFactory>();
            services.AddTransient<IExportServiceDataProvider, ExportServiceDataProvider>();
            services.AddTransient<IBinaryDataSource, BinaryDataSource>();
            services.AddTransient<IJobsStatusReporting, JobsStatusReporting>();
            services.AddTransient<IExportArchiveHandleService, ExportArchiveHandleService>();
            services.AddTransient<IDdiMetadataAccessor, DdiMetadataAccessor>();
            services.AddTransient<IDdiMetadataFactory, DdiMetadataFactory>();
            services.AddTransient<IMetaDescriptionFactory, MetaDescriptionFactory>();
            services.AddTransient<IExportJob, ExportJob>();
            services.AddTransient<IDatabaseSchemaService, DatabaseSchemaService>();
            services.AddSingleton<InterviewDataExportBulkCommandBuilderSettings>();
            services.AddTransient<IInterviewDataExportBulkCommandBuilder, InterviewDataExportBulkCommandBuilder>();
            services.AddTransient<ICommandExecutor, CommandExecutor>();
            services.AddTransient<IInterviewReferencesStorage, InterviewReferencesStorage>();
            services.AddTransient<IDatabaseSchemaCommandBuilder, DatabaseSchemaCommandBuilder>();
            services.AddTransient<IUserStorage, UserStorage>();
            services.AddTransient<IPdfExporter, PdfExporter>();
            services.AddTransient<IQuestionnaireBackupExporter, QuestionnaireBackupExporter>();

            services.AddTransient<IEventProcessor, EventsProcessor>();
            services.AddScoped<ITenantContext, TenantContext>();

            RegisterFunctionalHandlers(services, typeof(InterviewSummaryDenormalizer));

            services.UseExportProcessHandlers();;

            FileStorageModule.Register(services, configuration);

            // options
            services
                .Configure<ExportServiceSettings>(configuration.GetSection("ExportSettings"))
                .PostConfigure<ExportServiceSettings>(c =>
                {
                    c.DirectoryPath = c.DirectoryPath.Replace("~", Directory.GetCurrentDirectory());
                });
        }

        public static void RegisterFunctionalHandlers(this IServiceCollection services, params Type[] implementingTypes)
        {
            services.Scan(scan => scan
                .FromAssembliesOf(implementingTypes)
                    .AddClasses(classes => classes.AssignableTo<IFunctionalHandler>())
                    .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
        }
    }
}
