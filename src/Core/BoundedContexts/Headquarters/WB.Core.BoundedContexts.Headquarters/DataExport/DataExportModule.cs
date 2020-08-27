using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class DataExportModule : IModule
    {
        public void Load(IIocRegistry services)
        {
            services.AddTransient<ICsvReader, CsvReader>();
            services.AddTransient<ICsvWriter, CsvWriter>();
            services.AddTransient<ICsvWriterService, CsvWriterService>();
            services.AddTransient<IDataExportStatusReader, DataExportStatusReader>();
            services.AddTransient<IExportSettings, ExportSettings>();
            services.AddTransient<IExportViewFactory, ExportViewFactory>();
            services.AddTransient<IInterviewsToExportViewFactory, InterviewsToExportViewFactory>();
            services.AddTransient<IQuestionnaireLabelFactory, QuestionnaireLabelFactory>();
            services.AddTransient<ITabularFormatExportService, ReadSideToTabularFormatExportService>();
        }
    }
}
