using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Export.Services;

namespace WB.Services.Export.ExportProcessHandlers
{
    public static class HeadquartersApiHelpers
    {
        public static Task<List<InterviewToExport>> GetInterviewsToExportAsync(this IHeadquartersApi api,
            ExportSettings settings)
        {
            return api.GetInterviewsToExportAsync(settings.QuestionnaireId, settings.Status, settings.FromDate, settings.ToDate);
        }
    }
}
