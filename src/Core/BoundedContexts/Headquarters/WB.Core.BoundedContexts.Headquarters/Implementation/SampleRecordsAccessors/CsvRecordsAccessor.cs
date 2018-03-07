using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.SampleRecordsAccessors
{
    public class CsvRecordsAccessor : IRecordsAccessor
    {
        private readonly Stream sampleStream;
        private readonly string delimiter = ",";
        public CsvRecordsAccessor(Stream sampleStream, string delimiter)
        {
            this.sampleStream = sampleStream;
            this.delimiter = delimiter;
        }

        public IEnumerable<string[]> Records
        {
            get
            {
                using (var fileReader = new StreamReader(this.sampleStream, Encoding.UTF8))
                {
                    using (var csvReader = new CsvReader(fileReader))
                    {
                        csvReader.Configuration.Delimiter = this.delimiter;
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
            }
        }
    }
}
