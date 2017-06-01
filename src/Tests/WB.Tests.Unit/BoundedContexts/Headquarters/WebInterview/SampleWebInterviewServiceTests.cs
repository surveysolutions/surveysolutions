using System;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    [TestOf(typeof(SampleWebInterviewService))]
    public class SampleWebInterviewServiceTests
    {
        [Test]
        public void When_no_interviews_created_for_questionnaire()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();

            var service = this.GetService();

            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();

            Assert.That(reader.FieldHeaders.Length, Is.EqualTo(2));
            Assert.That(reader.FieldHeaders[0], Is.EqualTo("interview__link"));
            Assert.That(reader.FieldHeaders[1], Is.EqualTo("id"));

            var csvReaderException = Assert.Throws<CsvReaderException>(() => reader.Read(), "Only header row should be written to the output file");
            Assert.That(csvReaderException.Message, Does.Contain("exhausted all records"));
        }

        [Test]
        public void When_generating_for_questionnaire_without_prefilled_questions_Should_ouput_links()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();

            var assignmentId = 5;
            var summary = Create.Entity.Assignment(id: assignmentId, questionnaireIdentity: questionnaireId, assigneeSupervisorId: Guid.NewGuid());

            IPlainStorageAccessor<Assignment> assignments = new InMemoryPlainStorageAccessor<Assignment>();
            assignments.Store(summary, summary.Id);

            var service = this.GetService(assignments: assignments);

            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();
            Assert.That(reader.FieldHeaders.Length, Is.EqualTo(2));
            Assert.That(reader.FieldHeaders[0], Is.EqualTo("interview__link"));
            Assert.That(reader.FieldHeaders[1], Is.EqualTo("id"));

            Assert.That(reader.GetField(0), Is.EqualTo($"http://baseurl/{assignmentId}/Start"));
            Assert.That(reader.GetField(1), Is.EqualTo($"{assignmentId}"));
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
            Guid questionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            
            var summary = Create.Entity.Assignment(id: 5, questionnaireIdentity: questionnaireId, assigneeSupervisorId: Guid.NewGuid());
            summary.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(summary, answer: "bla", questionId: questionId));

            IPlainStorageAccessor<Assignment> assignments = new InMemoryPlainStorageAccessor<Assignment>();
            assignments.Store(summary, summary.Id);

            var questionnaires = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId,
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(questionId, text: prefilledQuestionTitle)));

            var service = this.GetService(assignments, questionnaires);

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

        private SampleWebInterviewService GetService(IPlainStorageAccessor<Assignment> assignments = null, IQuestionnaireStorage questionnaireStorage = null)
        {
            return new SampleWebInterviewService(assignments ?? new InMemoryPlainStorageAccessor<Assignment>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
        }
    }
}