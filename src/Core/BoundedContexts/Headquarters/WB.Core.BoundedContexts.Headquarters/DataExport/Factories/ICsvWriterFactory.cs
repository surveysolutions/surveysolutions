using System.IO;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public interface ICsvWriterFactory
    {
        ICsvWriterService OpenCsvWriter(Stream stream, string delimiter = ",");
    }
}