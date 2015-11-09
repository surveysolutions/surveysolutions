using StatData.Core;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal interface ITabFileReader
    {
        IDatasetMeta GetMetaFromTabFile(string path);
        string[,] GetDataFromTabFile(string path);
    }
}
