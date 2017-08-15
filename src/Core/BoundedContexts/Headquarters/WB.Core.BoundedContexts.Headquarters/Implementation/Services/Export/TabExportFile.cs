using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class TabExportFile : ExportFile
    {
        public override byte[] GetFileBytes(string[] headers, object[][] data)
        {
            var sb = new StringBuilder();
            using (var csvWriter = new CsvWriter(new StringWriter(sb), this.CreateCsvConfiguration()))
            {
                foreach (var header in headers)
                {
                    csvWriter.WriteField(header);
                }
                csvWriter.NextRecord();

                foreach (var row in data)
                {
                    foreach (var column in row)
                    {
                        csvWriter.WriteField(column ?? "");
                    }
                    csvWriter.NextRecord();
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private CsvConfiguration CreateCsvConfiguration() => new CsvConfiguration
        {
            HasHeaderRecord = false,
            TrimFields = true,
            IgnoreQuotes = false,
            Delimiter = "\t"
        };

        public override string MimeType => "text/tab-separated-values";
        public override string FileExtension => ".tsv";
    }
}