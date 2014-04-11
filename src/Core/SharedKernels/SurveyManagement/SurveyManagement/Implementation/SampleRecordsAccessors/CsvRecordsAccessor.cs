using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.SampleRecordsAccessors
{
    public class CsvRecordsAccessor : IRecordsAccessor
    {
        private readonly Stream sampleStream;

        public CsvRecordsAccessor(Stream sampleStream)
        {
            this.sampleStream = sampleStream;
        }

        public IEnumerable<string[]> Records
        {
            get
            {
                using (var fileReader = new StreamReader(this.sampleStream))
                {
                    using (var csvReader = new CsvReader(fileReader))
                    {
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
