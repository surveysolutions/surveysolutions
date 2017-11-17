using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class CsvReader : ICsvReader
    {
        public IEnumerable<T> ReadAll<T>(Stream csvFileStream, string delimiter, bool hasHeaderRow = true)
        {
            using (var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream),
                new Configuration
                {
                    MissingFieldFound = null,
                    Delimiter = delimiter,
                    HasHeaderRecord = hasHeaderRow
                }))
            {
                reader.Read();
                reader.ReadHeader();
                foreach (var record in reader.GetRecords<T>())
                    yield return record;
            }
        }

        public string[] ReadHeader(Stream csvFileStream, string delimiter)
        {
            using (var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream),
                new Configuration
                {
                    MissingFieldFound = null,
                    Delimiter = delimiter
                }))
            {
                reader.Read();
                reader.ReadHeader();
                return reader.Context.HeaderRecord;
            }
        }
    }
}
