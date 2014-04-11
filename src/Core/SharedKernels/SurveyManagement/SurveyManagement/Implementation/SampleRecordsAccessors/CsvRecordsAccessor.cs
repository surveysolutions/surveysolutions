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
                bool isFirst = true;
                using (var fileReader = new StreamReader(this.sampleStream))
                {
                    using (var csvReader = new CsvReader(fileReader))
                    {
                        while (csvReader.Read())
                        {
                            if (isFirst)
                            {
                                yield return csvReader.FieldHeaders;
                                isFirst = false;
                            }
                            yield return csvReader.CurrentRecord;
                        }

                    }
                }
            }
        }
    }
}
