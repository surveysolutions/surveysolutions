using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class CsvInterviewExporter
    {
        private readonly string delimiter = ",";

        public void AddRecord(string filePath, InterviewDataExportLevelView items)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Append))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.Delimiter = delimiter;

                foreach (var item in items.Records)
                {
                    writer.WriteField(item.InterviewId);
                    writer.WriteField(item.RecordId);

                    foreach (var exportedQuestion in item.Questions)
                    {
                        foreach (string itemValue in exportedQuestion.Answers)
                        {
                            writer.WriteField(itemValue);
                        }
                    }

                    //      writer.WriteField(item.ParentRecordId.HasValue ? item.ParentRecordId.ToString() : string.Empty);
                    writer.NextRecord();
                }

                streamWriter.Flush();
            }
        }

        public byte[] CreateHeader(HeaderStructureForLevel header)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.Delimiter = this.delimiter;

                writer.WriteField("InterviewId");
                writer.WriteField(header.LevelIdColumnName);

                foreach (ExportedHeaderItem question in header.HeaderItems.Values)
                {
                    foreach (var columnName in question.ColumnNames)
                    {
                        writer.WriteField(columnName);
                    }
                }

                writer.NextRecord();
                streamWriter.Flush();
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }
    }
}
