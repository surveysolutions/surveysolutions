using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class ExportGeographySettings : AppSetting
    {
        public static readonly string ExportGeographySettingsKey = "exportgeographysettings";

        public ExportGeographySettings() { }

        public ExportGeographySettings(GeographyExportFormat format)
        {
            Format = format;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public GeographyExportFormat Format { get; set; } = GeographyExportFormat.Wkt;
    }
}
