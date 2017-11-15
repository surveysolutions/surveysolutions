using System.Collections.Generic;
using System.IO;
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
                        var isRead = csvReader.Read();

                        if (csvReader.Context.HeaderRecord != null && csvReader.Context.HeaderRecord.Length > 0)
                        {
                            yield return csvReader.Context.HeaderRecord;
                        }

                        while (isRead)
                        {
                            yield return csvReader.Context.Record;
                            isRead = csvReader.Read();
                        }
                    }
                }
            }
        }
    }
}
