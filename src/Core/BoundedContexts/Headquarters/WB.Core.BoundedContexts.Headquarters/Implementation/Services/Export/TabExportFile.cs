using System.Globalization;
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
            var sb = new StringBuilder();
            using (var csvWriter = new CsvWriter(new StringWriter(sb), this.CreateCsvConfiguration()))
            {
                WriteRow(report.Headers);

                if (report.Totals != null)
                {
                    WriteRow(report.Totals);
                }

                foreach (var row in report.Data)
                {
                    WriteRow(row);
                }

                void WriteRow<T>(T[] row) where T : class
                {
                    foreach (var column in row)
                    {
                        csvWriter.WriteField((object)column ?? string.Empty);
                    }

                    csvWriter.NextRecord();
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private CsvConfiguration CreateCsvConfiguration() => new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            Mode = CsvMode.RFC4180,
            Delimiter = Delimiter,
        };

        public override string MimeType => "text/tab-separated-values";
        public override string FileExtension => Extention;

        public const string Extention = ".tab";
        public const string Delimiter = "\t";
    }
}
