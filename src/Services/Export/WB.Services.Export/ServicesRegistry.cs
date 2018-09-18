using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Refit;
using WB.Services.Export.CsvExport;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.DescriptionGenerator;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Infrastructure.Implementation;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Questionnaire.Services.Implementation;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Implementation;

namespace WB.Services.Export
{
    public class ServicesRegistry
    {
        public static void Configure(IServiceCollection services)
        {
            // Transients
            services.AddTransient<IFileSystemAccessor, FileSystemAccessor>();
            services.AddTransient<ICsvWriter, CsvWriter>();
            services.AddTransient<ICsvExport, CsvExport.Implementation.CsvExport>();
            services.AddTransient<IProductVersion, ProductVersion>();
            services.AddTransient<ICommentsExporter, CommentsExporter>();
            services.AddTransient<IQuestionnaireExportStructureFactory, QuestionnaireExportStructureFactory>();
            services.AddTransient<IDiagnosticsExporter, DiagnosticsExporter>();
            services.AddTransient<IQuestionnaireStorage, QuestionnaireStorage>();
            services.AddTransient<IInterviewActionsExporter, InterviewActionsExporter>();
            services.AddTransient<IInterviewsExporter, InterviewsExporter>();
            services.AddTransient<IInterviewFactory, InterviewFactory>();
            services.AddTransient<IInterviewErrorsExporter, InterviewErrorsExporter>();
            services.AddTransient<IExportQuestionService, ExportQuestionService>();
            services.AddTransient<IDescriptionGenerator, DescriptionGenerator.DescriptionGenerator>();
            services.AddTransient<IEnvironmentContentService, StataEnvironmentContentService>();

            // Singletons
            services.AddSingleton<ICache, Cache>();
        }
    }
}
