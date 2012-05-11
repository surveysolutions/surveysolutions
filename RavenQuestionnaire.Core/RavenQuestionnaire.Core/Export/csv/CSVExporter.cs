using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

namespace RavenQuestionnaire.Core.Export.csv
{
    /// <summary>
    /// Implements comma-separated values export format.
    /// </summary>
    public class CSVExporter : IExportProvider
    {
        public CSVExporter(char delimeter)
        {
            Delimeter = delimeter;
        }

        public char Delimeter { private set; get; }

        public bool DoExport(Dictionary<Guid, string> template, CompleteQuestionnaireExportView records, string fileName)
        {
            using (var memoryStream =  DoExportToStream(template, records))
            {
                using (var fileStream = File.Create(fileName))
                {
                    memoryStream.CopyTo(fileStream);
                }
            }
            return true;
        }

        public Stream DoExportToStream(Dictionary<Guid, string> template, CompleteQuestionnaireExportView records)
        {
            Stream result = new MemoryStream();

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {

                writer.Configuration.Delimiter = Delimeter;
                writer.WriteField("ID");//templated column for ID
                
                //build up header
                foreach (var question in template.Values)
                {
                    writer.WriteField(question);
                }
                writer.NextRecord();

                //iterate over records
                foreach (var item in records.Items)
                {
                    writer.WriteField(item.Id);
                    foreach (var guid in template.Keys)
                    {
                 /*     TODO  var completeAnswer = item.CompleteAnswers.FirstOrDefault(a => a.QuestionPublicKey == guid);
                        writer.WriteField(completeAnswer != null
                                              ? completeAnswer.AnswerValue?? completeAnswer.AnswerText
                                              : null);*/
                    }
                    writer.NextRecord();
                }

                memoryStream.Position = 0;
                memoryStream.CopyTo(result);

                result.Position = 0;
                return result;
            }
        }
    }
}
