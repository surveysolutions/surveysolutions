using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class SampleWebInterviewService : ISampleWebInterviewService
    {
        protected internal const string AssignmentLink = "assignment__link";
        protected internal const string AssignmentId = "assignment__id";
        private readonly IAssignmentsService assignmentsService;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public SampleWebInterviewService(IAssignmentsService assignmentsService, IQuestionnaireStorage questionnaireStorage)
        {
            this.assignmentsService = assignmentsService;
            this.questionnaireStorage = questionnaireStorage;
        }

        public byte[] Generate(QuestionnaireIdentity questionnaire, string baseUrl)
        {
            var assignmentsToExport = this.GetAssignments(questionnaire);

            var csvConfiguration = new Configuration
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                MissingFieldFound = null
            };

            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaire(questionnaire, null);

            using (MemoryStream output = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(output))
                {
                    using (CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration))
                    {
                        var header = this.WriteHeaderRow(csvWriter, questionnaireDocument, assignmentsToExport);
                        foreach (var assignment in assignmentsToExport)
                        {
                            PushAssignment(header, assignment, questionnaireDocument, csvWriter, baseUrl);
                        }
                    }
                }

                return output.ToArray();
            }
        }

        private List<string> WriteHeaderRow(CsvWriter csvWriter, IQuestionnaire questionnaire, List<Assignment> sampleAssignment)
        {
            List<string> header = new List<string>();
            header.Add(AssignmentLink);
            header.Add(AssignmentId);

            if (sampleAssignment != null)
            {
                foreach (var questionItem in sampleAssignment.SelectMany(x => x.IdentifyingData)
                    .Select(x => x.Identity.Id).Distinct()
                    .Select(questionnaire.GetQuestionVariableName))
                {
                    header.Add(questionItem);
                }
            }

            foreach (var column in header)
            {
                csvWriter.WriteField(column);
            }

            csvWriter.NextRecord();
            return header;
        }

        private static void PushAssignment(List<string> header,
            Assignment assignment,
            IQuestionnaire questionnaire,
            CsvWriter csvWriter,
            string baseUrl)
        {
            foreach (var columnName in header)
            {
                if (columnName.Equals(AssignmentLink, StringComparison.Ordinal))
                {
                    csvWriter.WriteField($"{baseUrl}/{assignment.Id}/Start");
                }
                else if (columnName.Equals(AssignmentId, StringComparison.Ordinal))
                {
                    csvWriter.WriteField(assignment.Id);
                }
                else
                {
                    var questionId = questionnaire.GetQuestionIdByVariable(columnName);
                    var answer = assignment.IdentifyingData.FirstOrDefault(x => x.Identity.Id == questionId);
                    csvWriter.WriteField(answer?.Answer ?? string.Empty);
                }
            }
            csvWriter.NextRecord();
        }

        private List<Assignment> GetAssignments(QuestionnaireIdentity questionnaire)
        {
            return this.assignmentsService.GetAssignmentsReadyForWebInterview(questionnaire);
        }
    }
}
