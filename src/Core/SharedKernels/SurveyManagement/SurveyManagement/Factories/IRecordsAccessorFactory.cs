using System.IO;
using WB.Core.SharedKernels.SurveyManagement.Implementation;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IRecordsAccessorFactory
    {
        IRecordsAccessor CreateRecordsAccessor(Stream sampleStream, string delimiter);
    }
}
