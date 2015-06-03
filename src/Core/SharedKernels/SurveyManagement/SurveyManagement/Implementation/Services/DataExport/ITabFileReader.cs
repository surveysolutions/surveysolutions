using StatData.Core;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    public interface ITabFileReader
    {
        IDatasetMeta GetMetaFromTabFile(string path);
        string[,] GetDataFromTabFile(string path);
    }
}
