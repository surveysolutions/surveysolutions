using StatData.Core;

namespace WB.Services.Export.Services
{
    internal interface ITabFileReader
    {
        IDatasetMeta GetMetaFromTabFile(string path);
        string[,] GetDataFromTabFile(string path);
    }
}