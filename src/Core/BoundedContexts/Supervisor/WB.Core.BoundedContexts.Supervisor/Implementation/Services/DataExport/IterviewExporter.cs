﻿using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class IterviewExporter : IExportProvider<InterviewDataExportLevelView>
    {
        private readonly char delimeter;

        public IterviewExporter(FileType exportingFileType)
        {
            this.delimeter = exportingFileType == FileType.Csv ? ',' : '\t';
        }

        public bool DoExport(InterviewDataExportLevelView items, string fileName)
        {
            byte[] bytes = this.DoExportToStream(items);

            using (FileStream fileStream = File.Create(fileName))
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }
            return true;
        }

        public byte[] DoExportToStream(InterviewDataExportLevelView items)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.Delimiter = this.delimeter.ToString();

               
                writer.WriteField("InterviewId");
                writer.WriteField("Id");

                foreach (ExportedHeaderItem question in items.Header.HeaderItems.Values)
                {
                    foreach (var columnName in question.ColumnNames)
                    {
                        writer.WriteField(columnName);
                    }
                }

             //   writer.WriteField("ForeignKey");
                writer.NextRecord();

                foreach (var item in items.Records)
                {
                    writer.WriteField(item.InterviewId);
                    writer.WriteField(item.RecordId);
                    foreach (var headerItem in items.Header.HeaderItems.Values)
                    {
                        var question = item.Questions.FirstOrDefault(q => q.QuestionId == headerItem.PublicKey);
                        if (question==null)
                        {
                            for (int i = 0; i < headerItem.ColumnNames.Count(); i++)
                            {
                                writer.WriteField(string.Empty);
                            }

                            continue;
                        }
                        foreach (string itemValue in question.Answers)
                        {
                            writer.WriteField(itemValue);
                        }
                    }

              //      writer.WriteField(item.ParentRecordId.HasValue ? item.ParentRecordId.ToString() : string.Empty);
                    writer.NextRecord();
                }

                streamWriter.Flush();
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }
    }
}
