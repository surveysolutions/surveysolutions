using StatData.Core;

namespace WB.Services.Export.Services
{
    public interface IDataQueryFactory
    {
        IDataQuery CreateDataQuery(string filePath);
    }
}
