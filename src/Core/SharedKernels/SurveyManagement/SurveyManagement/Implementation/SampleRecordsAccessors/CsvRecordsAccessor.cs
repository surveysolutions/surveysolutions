using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.SampleRecordsAccessors
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

                        if (csvReader.FieldHeaders != null && csvReader.FieldHeaders.Length > 0)
                        {
                            yield return csvReader.FieldHeaders;
                        }

                        while (isRead)
                        {
                            yield return csvReader.CurrentRecord;
                            isRead = csvReader.Read();
                        }
                    }
                }
            }
        }
    }
}
