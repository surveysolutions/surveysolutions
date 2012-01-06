using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Export.csv
{
    /// <summary>
    /// Implements comma-separated values export format.
    /// </summary>
    public class CSVExporter : IExportProvider
    {
        public bool DoExport(Dictionary<Guid, string> template, CompleteQuestionnaireBrowseView records, string fileName)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var streamReader = new StreamReader(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.WriteField("ID");//templated column for ID

                foreach (var question in template.Values)
                {
                    writer.WriteField(question);
                }

                writer.NextRecord();

                foreach (var item in records.Items)
                {
                    writer.WriteField(item.Id);

                    foreach (var guid in template.Keys)
                    {
                        writer.WriteField(null);
                    }
                    writer.NextRecord();
                }
                

                memoryStream.Position = 0;

                //Console.WriteLine(streamReader.ReadToEnd());
            }

            return true;
        }

    }
}
