using StatData.Core;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal interface IDataQueryFactory
    {
        IDataQuery CreateDataQuery(string filePath);
    }
}