using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public interface ICsvWriter
    {
        void WriteData(string filePath, IEnumerable<string[]> records, string delimiter);
    }
}
