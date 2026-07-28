using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IGeographySerializer
    {
        string Serialize(Area? area, GeometryType? geometryType, GeographyExportFormat format);
    }
}
