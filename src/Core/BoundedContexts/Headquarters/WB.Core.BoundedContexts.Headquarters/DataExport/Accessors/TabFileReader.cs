using StatData.Core;
using StatData.Readers;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class TabFileReader : ITabFileReader
    {
        public IDatasetMeta GetMetaFromTabFile(string path)
        {
            return TabReader.GetMeta(path);
        }

        public string[,] GetDataFromTabFile(string path)
        {
            return new TabReader().GetData(path);
        }
    }
}