using StatData.Core;

namespace WB.Services.Export.Services
{
    public interface ITabFileReader
    {
        IDatasetMeta GetMetaFromTabFile(string path);
        string[,] GetDataFromTabFile(string path);
    }
}
