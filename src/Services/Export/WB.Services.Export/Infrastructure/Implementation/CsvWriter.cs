using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Infrastructure.Implementation
{
    public class CsvWriter : ICsvWriter
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public CsvWriter(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public ICsvWriterService OpenCsvWriter(Stream stream, string delimiter = ",")
        {
            return new CsvWriterService(stream, delimiter);
        }

        public void WriteData(string filePath, IEnumerable<string[]> records, string delimiter)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(filePath, true))
            using (var tabWriter = this.OpenCsvWriter(fileStream, delimiter))
            {
                foreach (var dataRow in records.Where(x => x != null))
                {
                    foreach (var cell in dataRow)
                    {
                        tabWriter.WriteField(cell.RemoveNewLine());
                    }

                    tabWriter.NextRecord();
                }
            }
        }
    }
}
