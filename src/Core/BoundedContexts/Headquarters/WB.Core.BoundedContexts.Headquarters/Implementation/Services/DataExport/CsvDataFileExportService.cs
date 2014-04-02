using System.IO;
using System.Text;
using CsvHelper;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DataExport
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
                writer.Configuration.Delimiter = this.delimiter;

                foreach (var item in items.Records)
                {
                    writer.WriteField(item.RecordId);

                    foreach (var exportedQuestion in item.Questions)
                    {
                        foreach (string itemValue in exportedQuestion.Answers)
                        {
                            writer.WriteField(itemValue);
                        }
                    }

                    writer.WriteField(item.ParentRecordId);
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
                writer.WriteField(header.LevelIdColumnName);

                foreach (ExportedHeaderItem question in header.HeaderItems.Values)
                {
                    foreach (var columnName in question.ColumnNames)
                    {
                        writer.WriteField(columnName);
                    }
                }
                writer.WriteField("ParentId");
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
