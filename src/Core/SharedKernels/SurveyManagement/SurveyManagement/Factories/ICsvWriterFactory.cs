using System.IO;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface ICsvWriterFactory
    {
        ICsvWriterService OpenCsvWriter(Stream stream);
    }
}