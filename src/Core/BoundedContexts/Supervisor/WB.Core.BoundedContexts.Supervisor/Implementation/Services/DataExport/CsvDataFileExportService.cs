using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class CsvDataFileExportService : IDataFileExportService
    {
        private readonly string delimiter = ",";

        public void AddRecord(InterviewDataExportLevelView items, string filePath)
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

        public void CreateHeader(HeaderStructureForLevel header, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
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
            }
        }

        public string GetInterviewExportedDataFileName(string levelName)
        {
            return string.Format("{0}.csv", levelName);
        }
    }
}
