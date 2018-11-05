using StatData.Core;

namespace WB.Services.Export.Services.Implementation
{
    internal class DataQueryFactory : IDataQueryFactory
    {
        public IDataQuery CreateDataQuery(string filePath)
        {
            return new TabStreamDataQuery(filePath);
        }
    }
}
