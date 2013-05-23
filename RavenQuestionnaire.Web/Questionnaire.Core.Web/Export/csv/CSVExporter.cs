// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CSVExporter.cs" company="">
//   
// </copyright>
// <summary>
//   Implements comma-separated values export format.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace Questionnaire.Core.Web.Export.csv
{
    using System;
    using System.IO;

    using CsvHelper;

    using Main.Core.Export;
    using Main.Core.View.Export;

    /// <summary>
    /// Implements comma-separated values export format.
    /// </summary>
    public class CSVExporter : IExportProvider<CompleteQuestionnaireExportView>
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
            byte[] bytes = this.DoExportToStream(records);

            /* using (Stream memoryStream = this.DoExportToStream(records))
            {*/
            using (FileStream fileStream = File.Create(fileName))
            {
                fileStream.Write(bytes, 0, bytes.Length);

                // memoryStream.CopyTo(fileStream);
            }

            // }
            return true;
        }

        /// <summary>
        /// The do export to stream.
        /// </summary>
        /// <param name="records">
        /// The records.
        /// </param>
        /// <returns>
        /// The System.IO.Stream.
        /// </returns>
        public byte[] DoExportToStream(CompleteQuestionnaireExportView records)
        {
            // Stream result = new MemoryStream();
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream,Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.Delimiter = this.Delimeter.ToString();

                writer.WriteField("PublicKey"); // templated column for ID

                // build up header
                foreach (HeaderItem question in records.Header)
                {
                    writer.WriteField(question.Caption);
                }

                writer.WriteField("ForeignKey");
                writer.NextRecord();

                // iterate over records
                foreach (CompleteQuestionnaireExportItem item in records.Items)
                {
                    writer.WriteField(item.PublicKey);
                    foreach (Guid guid in records.Header.Keys)
                    {
                        /*     TODO  var completeAnswer = item.CompleteAnswers.FirstOrDefault(a => a.QuestionPublicKey == guid);*/
                        // var firstOrDefault = item.CompleteQuestions.FirstOrDefault(a => a.PublicKey == guid);
                        foreach (string itemValue in item.Values[guid])
                        {
                            writer.WriteField(itemValue);
                        }
                    }

                    writer.WriteField(item.Parent);
                    writer.NextRecord();
                }

                streamWriter.Flush();
                memoryStream.Position = 0;

                // memoryStream.CopyTo(result);

                // result.Position = 0;
                return memoryStream.ToArray();
            }
        }

        #endregion
    }
}