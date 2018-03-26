using System.Collections.Generic;
using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public interface ICsvReader
    {
        IEnumerable<T> ReadAll<T>(Stream csvFileStream, string delimiter, bool hasHeaderRow = true);
        string[] ReadHeader(Stream csvFileStream, string delimiter);
        IEnumerable<string[]> ReadRowsWithHeader(Stream csvFileStream, string delimiter);
        IEnumerable<dynamic> GetRecords(Stream csvFileStream, string delimiter);
    }
}