using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class SampleWebInterviewService : ISampleWebInterviewService
    {
        private static readonly Regex TitlesCleanupRegex = new Regex(@"(\r\n?|\n|\t)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IQueryableReadSideRepositoryReader<QuestionAnswer> answers;
        private readonly InterviewDataExportSettings exportSettings;

        class InterviewReference
        {
            public InterviewReference(string id, string key)
            {
                this.Id = id;
                this.Key = key;
            }

            public string Id { get; }
            public string Key { get; }
        }

        class PrefilledQuestionReference
        {
            public PrefilledQuestionReference(string interviewId, string answer)
            {
                this.InterviewId = interviewId;
                this.Answer = answer;
            }

            public string InterviewId { get; }

            public string Answer { get; }
        }

        public SampleWebInterviewService(IQueryableReadSideRepositoryReader<InterviewSummary> summaries,
            IQueryableReadSideRepositoryReader<QuestionAnswer> answers,
            InterviewDataExportSettings exportSettings)
        {
            this.summaries = summaries;
            this.answers = answers;
            this.exportSettings = exportSettings;
        }

        public byte[] Generate(QuestionnaireIdentity questionnaire, string baseUrl)
        {
            var interviewIdsToExport = this.GetInterviewIds(questionnaire);

            var csvConfiguration = new CsvConfiguration
            {
                HasHeaderRecord = true,
                TrimFields = true,
                IgnoreQuotes = false,
                Delimiter = "\t",
                WillThrowOnMissingField = false
            };
            using (MemoryStream output = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(output))
                {
                    using (CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration))
                    {
                        this.WriteHeaderRow(csvWriter, interviewIdsToExport.FirstOrDefault());

                        foreach (var interviewsBatch in interviewIdsToExport.Batch(20))
                        {
                            string[] array = interviewsBatch.Select(x => x.Id).ToArray();
                            var prefilledQuestions = this.GetPrefilledQuestionsBatch(array);
                            PushInterview(interviewsBatch, csvWriter, prefilledQuestions, baseUrl);
                        }
                    }
                }

                return output.ToArray();
            }
        }

        private List<PrefilledQuestionReference> GetPrefilledQuestionsBatch(string[] array)
        {
            var prefilledQuestions = this.answers.Query(_ => _
                .Where(x => array.Contains(x.InterviewSummary.SummaryId))
                .OrderBy(x => x.InterviewSummary.SummaryId)
                .ThenBy(x => x.Position)
                .Select(x => new PrefilledQuestionReference(x.InterviewSummary.SummaryId, x.Answer))
                .ToList());
            return prefilledQuestions;
        }

        private void WriteHeaderRow(CsvWriter csvWriter, InterviewReference sampleInterviewId)
        {
            csvWriter.WriteField("interview__link");
            csvWriter.WriteField("interview__key");
            csvWriter.WriteField("id");

            if (sampleInterviewId != null)
            {
                var titles = this.answers.Query(_ => _
                    .Where(x => x.InterviewSummary.SummaryId == sampleInterviewId.Id)
                    .OrderBy(x => x.Position)
                    .Select(x => x.Title)
                    .ToList());

                foreach (var questionItem in titles)
                {
                    csvWriter.WriteField(TitlesCleanupRegex.Replace(questionItem.RemoveHtmlTags(), ""));
                }
            }

            csvWriter.NextRecord();
        }

        private static void PushInterview(IEnumerable<InterviewReference> interviewsBatch,
            CsvWriter csvWriter,
            List<PrefilledQuestionReference> prefilledQuestions,
            string baseUrl)
        {
            foreach (var interviewReference in interviewsBatch)
            {
                csvWriter.WriteField($"{baseUrl}/{interviewReference.Id}/Cover");
                csvWriter.WriteField(interviewReference.Key ?? "");
                csvWriter.WriteField(interviewReference.Id ?? "");

                foreach (var prefilledQuestion in prefilledQuestions.Where(x => x.InterviewId == interviewReference.Id))
                {
                    csvWriter.WriteField(prefilledQuestion.Answer);
                }
                csvWriter.NextRecord();
            }
        }

        private List<InterviewReference> GetInterviewIds(QuestionnaireIdentity questionnaire)
        {
            var interviewIdsToExport = new List<InterviewReference>();

            string lastRecivedId = null;
            while (true)
            {
                var ids = this.summaries.Query(_ => _
                    .Where(x => x.QuestionnaireId == questionnaire.QuestionnaireId &&
                        x.QuestionnaireVersion == questionnaire.Version &&
                        x.Status == InterviewStatus.InterviewerAssigned)
                    .OrderBy(x => x.InterviewId)
                    .Where(x => lastRecivedId == null || x.SummaryId.CompareTo(lastRecivedId) > 0)
                    .Select(x => new { x.SummaryId, x.Key })
                    .Take(this.exportSettings.InterviewIdsQueryBatchSize)
                    .ToList());

                if (ids.Count == 0) break;

                interviewIdsToExport.AddRange(ids.Select(x => new InterviewReference(x.SummaryId, x.Key)));
                lastRecivedId = ids.Select(x => x.SummaryId).Last();
            }
            return interviewIdsToExport;
        }
    }
}