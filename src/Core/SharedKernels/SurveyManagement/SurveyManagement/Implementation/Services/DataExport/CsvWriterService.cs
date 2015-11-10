﻿using System;
using System.IO;
using System.Text;
using CsvHelper;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
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
            this.csvWriter = new CsvWriter(streamWriter);
            this.csvWriter.Configuration.Delimiter = this.delimiter;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                streamWriter.Flush();
                csvWriter.Dispose();
                streamWriter.Dispose();
            }
        }
        public void WriteField<T>(T cellValue)
        {
            csvWriter.WriteField(cellValue);
        }

        public void NextRecord()
        {
            csvWriter.NextRecord();
        }
    }
}
