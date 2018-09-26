using StatData.Core;

namespace WB.Services.Export.Services
{
    internal interface IDataQueryFactory
    {
        IDataQuery CreateDataQuery(string filePath);
    }
}