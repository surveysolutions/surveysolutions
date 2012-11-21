// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CSVExporter.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Implements comma-separated values export format.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Export.csv
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CsvHelper;

    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

    /// <summary>
    /// Implements comma-separated values export format.
    /// </summary>
    public class CSVExporter : IExportProvider
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CSVExporter"/> class.
        /// </summary>
        /// <param name="delimeter">
        /// The delimeter.
        /// </param>
        public CSVExporter(char delimeter)
        {
            this.Delimeter = delimeter;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the delimeter.
        /// </summary>
        public char Delimeter { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The do export.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="records">
        /// The records.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool DoExport(CompleteQuestionnaireExportView records, string fileName)
        {
            using (Stream memoryStream = this.DoExportToStream(records))
            {
                using (FileStream fileStream = File.Create(fileName))
                {
                    memoryStream.CopyTo(fileStream);
                }
            }

            return true;
        }

        /// <summary>
        /// The do export to stream.
        /// </summary>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="records">
        /// The records.
        /// </param>
        /// <returns>
        /// The System.IO.Stream.
        /// </returns>
        public Stream DoExportToStream( CompleteQuestionnaireExportView records)
        {
            Stream result = new MemoryStream();

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.Delimiter = this.Delimeter;
                writer.WriteField("ID"); // templated column for ID

                // build up header
                foreach (string question in records.Header.Values)
                {
                    writer.WriteField(question);
                }

                writer.NextRecord();

                // iterate over records
                foreach (CompleteQuestionnaireExportItem item in records.Items)
                {
                    foreach (Guid guid in records.Header.Keys)
                    {
                        /*     TODO  var completeAnswer = item.CompleteAnswers.FirstOrDefault(a => a.QuestionPublicKey == guid);*/
                        // var firstOrDefault = item.CompleteQuestions.FirstOrDefault(a => a.PublicKey == guid);
                        if (item.Values.ContainsKey(guid))
                        {
                            writer.WriteField(item.Values[guid]);
                        }
                    }

                    writer.NextRecord();
                }

                memoryStream.Position = 0;
                memoryStream.CopyTo(result);

                result.Position = 0;
                return result;
            }
        }

        #endregion
    }
}