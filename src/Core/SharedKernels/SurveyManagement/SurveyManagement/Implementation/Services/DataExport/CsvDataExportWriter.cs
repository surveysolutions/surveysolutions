using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class CsvDataExportWriter : IDataExportWriter
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriterFactory csvWriterFactory;

        public CsvDataExportWriter(IFileSystemAccessor fileSystemAccessor, ICsvWriterFactory csvWriterFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriterFactory = csvWriterFactory;
        }
         
        public void AddRecords(InterviewDataExportLevelView items, string filePath)
        {
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(filePath, true))
            using (var writer = csvWriterFactory.OpenCsvWriter(fileStream))
            {
                foreach (var item in items.Records)
                {
                    writer.WriteField(item.RecordId);

                    foreach (var referenceValue in item.ReferenceValues)
                    {
                        writer.WriteField(referenceValue);
                    }

                    foreach (var exportedQuestion in item.Questions)
                    {
                        foreach (string itemValue in exportedQuestion.Answers)
                        {
                            writer.WriteField(itemValue);
                        }
                    }
                    foreach (var parentRecordId in item.ParentRecordIds)
                    {
                        writer.WriteField(parentRecordId);
                    }
                    writer.NextRecord();
                }
            }
        }

        public void AddActionRecord(InterviewActionExportView action, string filePath)
        {
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(filePath, true))
            using (var writer = csvWriterFactory.OpenCsvWriter(fileStream))
            {
                writer.WriteField(action.InterviewId);
                writer.WriteField(action.Action);
                writer.WriteField(action.Originator);
                writer.WriteField(action.Role);
                writer.WriteField(action.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                writer.WriteField(action.Timestamp.ToString("T", CultureInfo.InvariantCulture));

                writer.NextRecord();
            }
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView items, string basePath)
        {
            throw new NotImplementedException();
        }

        public void CreateStructure(QuestionnaireExportStructure header, string basePath)
        {
            throw new NotImplementedException();
        }

        public string[] GetAllDataFiles(string basePath, Func<string, string> fileNameCreationFunc)
        {
            throw new NotImplementedException();
        }

        public string[] GetApprovedDataFiles(string basePath, Func<string, string> fileNameCreationFunc)
        {
            throw new NotImplementedException();
        }

        public void DeleteInterviewRecords(string basePath, Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void BatchInsert(string basePath, IEnumerable<InterviewDataExportView> interviewDatas, IEnumerable<InterviewActionExportView> interviewActions)
        {
            throw new NotImplementedException();
        }

        public void CreateHeader(HeaderStructureForLevel header, string filePath)
        {
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(filePath, false))
            using (var writer = csvWriterFactory.OpenCsvWriter(fileStream))
            {
                writer.WriteField(header.LevelIdColumnName);

                if (header.IsTextListScope)
                {
                    foreach (var name in header.ReferencedNames)
                    {
                        writer.WriteField(name);
                    }
                }

                foreach (ExportedHeaderItem question in header.HeaderItems.Values)
                {
                    foreach (var columnName in question.ColumnNames)
                    {
                        writer.WriteField(columnName);
                    }
                }

                for (int i = 0; i < header.LevelScopeVector.Length; i++)
                {
                    writer.WriteField(string.Format("ParentId{0}", i + 1));
                }
                writer.NextRecord();
            }
        }

        public void CreateHeaderForActionFile(string filePath)
        {
            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(filePath, false))
             using (var writer = csvWriterFactory.OpenCsvWriter(fileStream))
            {   
                writer.WriteField("Id");
                writer.WriteField("Action");
                writer.WriteField("Originator");
                writer.WriteField("Role");
                writer.WriteField("Date");
                writer.WriteField("Time");

                writer.NextRecord();
            }
        }

        public string GetInterviewExportedDataFileName(string levelName)
        {
            return string.Format("{0}.csv", levelName);
        }

        public string GetInterviewActionFileName()
        {
            return "interview_actions.csv";
        }
    }
}
