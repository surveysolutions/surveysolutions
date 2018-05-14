using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class TabExportFile : ExportFile
    {
        public override byte[] GetFileBytes(ReportView report)
        {
            var headers = report.Headers;
            var data = report.Data;
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

        private Configuration CreateCsvConfiguration() => new Configuration
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            IgnoreQuotes = false,
            Delimiter = Delimiter,
        };

        public override string MimeType => "text/tab-separated-values";
        public override string FileExtension => Extention;

        public const string Extention = ".tab";
        public const string Delimiter = "\t";
    }
}
