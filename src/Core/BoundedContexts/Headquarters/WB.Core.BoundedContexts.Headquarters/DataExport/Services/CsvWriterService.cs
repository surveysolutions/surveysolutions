using System;
using System.IO;
using System.Text;
using CsvHelper;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class CsvWriterService : ICsvWriterService
    {
        private readonly StreamWriter streamWriter;
        private readonly CsvWriter csvWriter;
        private readonly string delimiter = ",";

        public CsvWriterService(Stream stream, string delimiter = ",")
        {
            this.delimiter = delimiter;
            this.streamWriter = new StreamWriter(stream, Encoding.UTF8);
            this.csvWriter = new CsvWriter(this.streamWriter);
            this.csvWriter.Configuration.Delimiter = this.delimiter;
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
