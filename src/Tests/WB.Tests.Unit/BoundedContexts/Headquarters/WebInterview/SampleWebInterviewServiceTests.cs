using System;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    [TestOf(typeof(SampleWebInterviewService))]
    public class SampleWebInterviewServiceTests 
    {
        [Test]
        public void When_generating_for_questionnaire_without_prefilled_questions_Should_ouput_links()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();
            Guid interviewId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var interviewKey = "key";

            var summary = Create.Entity.InterviewSummary(questionnaireId: questionnaireId.QuestionnaireId,
                questionnaireVersion: questionnaireId.Version,
                key: interviewKey,
                status: InterviewStatus.InterviewerAssigned,
                interviewId: interviewId);

            TestInMemoryWriter<InterviewSummary> summaries =
                new TestInMemoryWriter<InterviewSummary>(interviewId.FormatGuid(), summary);

            var service = this.GetService(summaries);

            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();
            Assert.That(reader.FieldHeaders.Length, Is.EqualTo(2));
            Assert.That(reader.FieldHeaders[0], Is.EqualTo("Link"));
            Assert.That(reader.FieldHeaders[1], Is.EqualTo("Interview Key"));

            Assert.That(reader.GetField(0), Is.EqualTo($"http://baseurl/{interviewId:N}/Cover"));
            Assert.That(reader.GetField(1), Is.EqualTo(interviewKey));
        }

        [Test]
        [TestCase("header \t 1", "header  1")]
        [TestCase("header \r 1", "header  1")]
        [TestCase("header \n 1", "header  1")]
        [TestCase("header \r\n 1", "header  1")]
        [TestCase("header <br /> 1", "header  1")]
        public void when_question_title_contains_not_allowed_characters_should_trim_it(string prefilledQuestionTitle, string expectedHeader)
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();
            var summary = Create.Entity.InterviewSummary(questionnaireId: questionnaireId.QuestionnaireId,
                questionnaireVersion: questionnaireId.Version,
                status: InterviewStatus.InterviewerAssigned);
            var prefilledQuestionAnswer = new QuestionAnswer
            {
                Id = 25,
                InterviewSummary = summary,
                Title = prefilledQuestionTitle,
                Answer = "bla"
            };
            summary.AnswersToFeaturedQuestions.Add(prefilledQuestionAnswer);

            TestInMemoryWriter<InterviewSummary> summaries =
                new TestInMemoryWriter<InterviewSummary>(summary.SummaryId, summary);
            TestInMemoryWriter<QuestionAnswer> answers = new TestInMemoryWriter<QuestionAnswer>(prefilledQuestionAnswer.Id.ToString(), prefilledQuestionAnswer);

            var service = this.GetService(summaries, answers);

            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();
            Assert.That(reader.FieldHeaders[2], Is.EqualTo(expectedHeader));
            Assert.That(reader.FieldHeaders[2], Is.EqualTo(expectedHeader));
        }

        private static CsvReader GetCsvReader(byte[] ouptutBytes)
        {
            var streamReader = new StreamReader(new MemoryStream(ouptutBytes), Encoding.UTF8);
            var reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                TrimFields = true,
                IgnoreQuotes = false,
                Delimiter = "\t",
                WillThrowOnMissingField = false
            });
            return reader;
        }

        private SampleWebInterviewService GetService(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries = null,
            IQueryableReadSideRepositoryReader<QuestionAnswer> answers = null,
            InterviewDataExportSettings exportSettings = null)
        {
            return new SampleWebInterviewService(
                summaries ?? new TestInMemoryWriter<InterviewSummary>(), 
                answers ?? new TestInMemoryWriter<QuestionAnswer>(),  
                exportSettings ?? new InterviewDataExportSettings {InterviewIdsQueryBatchSize = 10});
        }
    }
}