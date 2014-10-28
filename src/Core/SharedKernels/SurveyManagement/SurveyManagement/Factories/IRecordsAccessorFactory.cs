using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Implementation;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IRecordsAccessorFactory
    {
        IRecordsAccessor CreateRecordsAccessor(Stream sampleStream, string delimiter);
    }
}
