using System.Collections.Generic;

namespace WB.Services.Export.Services
{
    public interface IExportServiceDataProvider
    {
        Dictionary<string, Dictionary<string,string>> GetServiceDataLabels();
    }
}