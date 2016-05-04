using StatData.Core;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class DataQueryFactory : IDataQueryFactory
    {
        public IDataQuery CreateDataQuery(string filePath)
        {
            return new TabStreamDataQuery(filePath);
        }
    }
}
