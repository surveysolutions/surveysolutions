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
                GetConfiguration(delimiter, hasHeaderRow)))
            {
                reader.Read();
                reader.ReadHeader();

                while (reader.Read())
                    yield return reader.GetRecord<T>();
            }
        }

        public IEnumerable<string[]> ReadRowsWithHeader(Stream csvFileStream, string delimiter)
        {
            using (var parser = new CsvHelper.CsvParser(new StreamReader(csvFileStream),
                GetConfiguration(delimiter, true), true))
            {
                while (true)
                {
                    var row = parser.Read();
                    if (row == null)
                        break;
                    yield return row;
                }
            }
        }

        public IEnumerable<dynamic> GetRecords(Stream csvFileStream, string delimiter)
        {
            csvFileStream.Seek(0, SeekOrigin.Begin);

            using (var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream),
                GetConfiguration(delimiter, true), true))
            {
                foreach (var record in reader.GetRecords<dynamic>())
                    yield return record;
            }
        }

        public string[] ReadHeader(Stream csvFileStream, string delimiter)
        {
            csvFileStream.Seek(0, SeekOrigin.Begin);

            using (var parser = new CsvHelper.CsvParser(new StreamReader(csvFileStream),
                GetConfiguration(delimiter, true), true))
            {
                return parser.Read() ?? new string[] { };
            }
        }

        private static Configuration GetConfiguration(string delimiter, bool hasHeaderRow)
        {
            return new Configuration
            {
                MissingFieldFound = null,
                Delimiter = delimiter,
                HasHeaderRecord = hasHeaderRow,
                PrepareHeaderForMatch = s => s.ToLower()
            };
        }
    }
}
