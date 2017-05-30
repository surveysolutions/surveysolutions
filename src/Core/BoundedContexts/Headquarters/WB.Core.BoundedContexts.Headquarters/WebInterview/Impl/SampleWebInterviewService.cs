using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class SampleWebInterviewService : ISampleWebInterviewService
    {
        private static readonly Regex TitlesCleanupRegex = new Regex(@"(\r\n?|\n|\t)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private IQuestionnaireStorage questionnaireStorage;

        public SampleWebInterviewService(IPlainStorageAccessor<Assignment> assignments, IQuestionnaireStorage questionnaireStorage)
        {
            this.assignments = assignments;
            this.questionnaireStorage = questionnaireStorage;
        }

        public byte[] Generate(QuestionnaireIdentity questionnaire, string baseUrl)
        {
            var assignmentsToExport = this.GetAssignments(questionnaire);

            var csvConfiguration = new CsvConfiguration
            {
                HasHeaderRecord = true,
                TrimFields = true,
                IgnoreQuotes = false,
                Delimiter = "\t",
                WillThrowOnMissingField = false
            };

            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaire, null);

            using (MemoryStream output = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(output))
                {
                    using (CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration))
                    {
                        this.WriteHeaderRow(csvWriter, questionnaireDocument, assignmentsToExport.FirstOrDefault());
                        foreach (var assignment in assignmentsToExport)
                        {
                            PushAssignment(assignment, csvWriter, baseUrl);
                        }
                    }
                }

                return output.ToArray();
            }
        }

        private void WriteHeaderRow(CsvWriter csvWriter, IQuestionnaire questionnaire, Assignment sampleAssignment)
        {
            csvWriter.WriteField("interview__link");
            csvWriter.WriteField("id");

            if (sampleAssignment != null)
            {
                foreach (var questionItem in sampleAssignment.IdentifyingData.Select(x => questionnaire.GetQuestionTitle(x.QuestionId)))
                {
                    csvWriter.WriteField(TitlesCleanupRegex.Replace(questionItem.RemoveHtmlTags(), ""));
                }
            }

            csvWriter.NextRecord();
        }

        private static void PushAssignment(Assignment assignment,
            CsvWriter csvWriter,
            string baseUrl)
        {
                csvWriter.WriteField($"{baseUrl}/{assignment.Id}/Start");
                csvWriter.WriteField(assignment.Id);

                foreach (var prefilledQuestion in assignment.IdentifyingData)
                {
                    csvWriter.WriteField(prefilledQuestion.Answer);
                }
                csvWriter.NextRecord();
        }

        private List<Assignment> GetAssignments(QuestionnaireIdentity questionnaire)
        {
            return this.assignments.Query(_ => _
                .Where(x => x.QuestionnaireId.QuestionnaireId == questionnaire.QuestionnaireId &&
                    x.QuestionnaireId.Version == questionnaire.Version &&
                    x.Responsible.ReadonlyProfile.SupervisorId != null)
                .OrderBy(x => x.Id)
                .Fetch(x => x.IdentifyingData)
                .ToList());
        }
    }
}