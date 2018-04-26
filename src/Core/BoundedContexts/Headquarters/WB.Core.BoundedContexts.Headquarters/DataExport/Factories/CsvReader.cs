using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public class CsvReader : ICsvReader
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
            using (var csvReader = new CsvHelper.CsvReader(new StreamReader(csvFileStream)))
            {
                csvReader.Configuration.Delimiter = delimiter;
                csvReader.Configuration.IgnoreQuotes = true;
                csvReader.Read();
                csvReader.ReadHeader();

                if (csvReader.Context.HeaderRecord != null && csvReader.Context.HeaderRecord.Length > 0)
                {
                    yield return csvReader.Context.HeaderRecord;
                }

                while (csvReader.Read())
                    yield return csvReader.Context.HeaderRecord.Select((x, index) =>
                        index < csvReader.Context.Record.Length ? csvReader.GetField(x) : null).ToArray();
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

            using (var parser = new CsvParser(new StreamReader(csvFileStream),
                GetConfiguration(delimiter, true), true))
            {
                return parser.Read() ?? new string[] { };
            }
        }

        private static Configuration GetConfiguration(string delimiter, bool hasHeaderRow)
            => new Configuration
            {
                MissingFieldFound = null,
                Delimiter = delimiter,
                HasHeaderRecord = hasHeaderRow,
                PrepareHeaderForMatch = s => s.ToLower(),
                IgnoreQuotes = true
            };
    }
}
