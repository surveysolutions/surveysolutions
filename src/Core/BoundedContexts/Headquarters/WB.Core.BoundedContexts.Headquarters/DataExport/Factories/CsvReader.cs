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
                new CsvConfiguration
                {
                    IsHeaderCaseSensitive = false,
                    WillThrowOnMissingField = false,
                    Delimiter = delimiter,
                    HasHeaderRecord = hasHeaderRow
                }))
            {
                foreach (var record in reader.GetRecords<T>())
                    yield return record;
            }
        }

        public string[] ReadHeader(Stream csvFileStream, string delimiter)
        {
            using (var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream),
                new CsvConfiguration
                {
                    IsHeaderCaseSensitive = false,
                    WillThrowOnMissingField = false,
                    Delimiter = delimiter,
                    HasHeaderRecord = true
                }))
            {
                reader.ReadHeader();
                return reader.FieldHeaders;
            }
        }
    }
}
