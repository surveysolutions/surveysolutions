using System.Collections.Generic;
using System.IO;

namespace WB.Services.Export.Infrastructure
{
    public interface ICsvWriter
    {
        ICsvWriterService OpenCsvWriter(Stream stream, string delimiter = ",");
        void WriteData(string filePath, IEnumerable<string[]> records, string delimiter);
    }
}