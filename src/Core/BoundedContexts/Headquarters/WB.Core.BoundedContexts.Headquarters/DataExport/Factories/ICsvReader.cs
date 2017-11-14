using System.Collections.Generic;
using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public interface ICsvReader
    {
        IEnumerable<T> ReadAll<T>(Stream csvFileStream, string delimiter, bool hasHeaderRow = true);
        string[] ReadHeader(Stream csvFileStream, string delimiter);
    }
}