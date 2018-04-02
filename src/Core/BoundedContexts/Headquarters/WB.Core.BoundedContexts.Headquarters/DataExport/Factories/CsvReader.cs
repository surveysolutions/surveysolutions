using System.Collections.Generic;
using System.IO;
using CsvHelper;
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
                    HasHeaderRecord = hasHeaderRow,
                    PrepareHeaderForMatch = s => s.ToLower()
                }))
            {
                reader.Read();
                reader.ReadHeader();

                while (reader.Read())
                    yield return reader.GetRecord<T>();
            }
        }

        public string[] ReadHeader(Stream csvFileStream, string delimiter)
        {
            using (var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream),
                new Configuration
                {
                    MissingFieldFound = null,
                    Delimiter = delimiter,
                    BadDataFound = delegate(IReadingContext context) {  }
                }))
            {
                reader.Read();

                return reader.ReadHeader() ? reader.Context.HeaderRecord : new string[0];
            }
        }
    }
}
