using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.SampleRecordsAccessors;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class CsvRecordsAccessorFactory : IRecordsAccessorFactory
    {
        public IRecordsAccessor CreateRecordsAccessor(Stream sampleStream, string delimiter = ",")
        {
            return new CsvRecordsAccessor(sampleStream, delimiter);
        }
    }
}
