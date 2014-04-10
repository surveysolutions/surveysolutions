﻿using System.IO;
using System.Text;
using CsvHelper;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class CsvDataFileExportService : IDataFileExportService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public CsvDataFileExportService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        private readonly string delimiter = ",";
         
        public void AddRecord(InterviewDataExportLevelView items, string filePath)
        {
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(filePath))
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
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(filePath))
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
