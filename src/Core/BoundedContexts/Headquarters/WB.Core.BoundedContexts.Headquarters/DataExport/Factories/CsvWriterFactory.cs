using System.IO;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class CsvWriterFactory : ICsvWriterFactory
    {
        public ICsvWriterService OpenCsvWriter(Stream stream, string delimiter = ",")
        {
            return new CsvWriterService(stream, delimiter);
        }
    }
}
