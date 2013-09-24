using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Export;
using Main.Core.View.Export;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class CSVIterviewExporter : IExportProvider<InterviewDataExportView>
    {
        public bool DoExport(InterviewDataExportView items, string fileName)
        {
            byte[] bytes = this.DoExportToStream(items);

            using (FileStream fileStream = File.Create(fileName))
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }
            return true;
        }

        public byte[] DoExportToStream(InterviewDataExportView items)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {

                writer.WriteField("InterviewId");
                writer.WriteField("Id");

                foreach (ExportedHeaderItem question in items.Header)
                {
                    writer.WriteField(question.Caption);
                }

             //   writer.WriteField("ForeignKey");
                writer.NextRecord();

                foreach (var item in items.Records)
                {
                    writer.WriteField(item.InterviewId);
                    writer.WriteField(item.RecordId);
                    foreach (Guid guid in items.Header.Keys)
                    {
                        if (!item.Questions.ContainsKey(guid))
                        {
                            for (int i = 0; i < items.Header.GetAvailableHeaderForQuestion(guid).Count(); i++)
                            {
                                writer.WriteField(string.Empty);
                            }

                            continue;
                        }
                        foreach (string itemValue in item.Questions[guid].Answers)
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
