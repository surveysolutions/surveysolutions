using System;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper.Configuration;

namespace WB.Services.Export.Infrastructure.Implementation
{
    internal class CsvWriterService : ICsvWriterService
    {
        private readonly StreamWriter streamWriter;
        private readonly CsvHelper.CsvWriter csvWriter;
        private readonly string delimiter = ",";

        public CsvWriterService(Stream stream, string delimiter = ",")
        {
            this.delimiter = delimiter;
            this.streamWriter = new StreamWriter(stream, Encoding.UTF8);
            this.csvWriter = new CsvHelper.CsvWriter(this.streamWriter, 
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = this.delimiter
                });
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.streamWriter.Flush();
                this.csvWriter.Dispose();
                this.streamWriter.Dispose();
            }
        }
        public void WriteField<T>(T cellValue)
        {
            this.csvWriter.WriteField(cellValue);
        }

        public void NextRecord()
        {
            this.csvWriter.NextRecord();
        }
    }
}
