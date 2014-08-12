using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class CsvWriterFactory : ICsvWriterFactory
    {
        public ICsvWriterService OpenCsvWriter(Stream stream)
        {
            return new CsvWriterService(stream);
        }
    }
}
