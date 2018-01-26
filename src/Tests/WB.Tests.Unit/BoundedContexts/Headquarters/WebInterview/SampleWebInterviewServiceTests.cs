using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

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
            reader.ReadHeader();

            Assert.That(reader.Context.HeaderRecord.Length, Is.EqualTo(2));
            Assert.That(reader.Context.HeaderRecord[0], Is.EqualTo("interview__link"));
            Assert.That(reader.Context.HeaderRecord[1], Is.EqualTo("id"));

            var isReaded = reader.Read();
            Assert.IsFalse(isReaded);
        }

        [Test]
        public void When_generating_for_questionnaire_without_prefilled_questions_Should_ouput_links()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();

            var assignmentId = 5;
            var assignment = Create.Entity.Assignment(id: assignmentId, questionnaireIdentity: questionnaireId, assigneeSupervisorId: Guid.NewGuid());
            var assignments = Create.Service.AssignmentService(assignment);
            var service = this.GetService(assignments: assignments);

            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();
            reader.ReadHeader();
            reader.Read();
            Assert.That(reader.Context.HeaderRecord.Length, Is.EqualTo(2));
            Assert.That(reader.Context.HeaderRecord[0], Is.EqualTo("interview__link"));
            Assert.That(reader.Context.HeaderRecord[1], Is.EqualTo("id"));

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
            var questionId = Create.Entity.Identity(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
            
            var assignment = Create.Entity.Assignment(id: 5, questionnaireIdentity: questionnaireId, assigneeSupervisorId: Guid.NewGuid());
            assignment.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment, answer: "bla", identity: questionId));

            IAssignmentsService assignments = Create.Service.AssignmentService(assignment);

            var questionnaires = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId,
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(questionId.Id, text: prefilledQuestionTitle)));

            var service = this.GetService(assignments, questionnaires);

            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();
            reader.ReadHeader();
            Assert.That(reader.Context.HeaderRecord[2], Is.EqualTo(expectedHeader));
            Assert.That(reader.Context.HeaderRecord[2], Is.EqualTo(expectedHeader));
        }

        private static CsvReader GetCsvReader(byte[] ouptutBytes)
        {
            var streamReader = new StreamReader(new MemoryStream(ouptutBytes), Encoding.UTF8);
            var reader = new CsvReader(streamReader, new Configuration
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                MissingFieldFound = null,
            });
            return reader;
        }

        private SampleWebInterviewService GetService(IAssignmentsService assignments = null, IQuestionnaireStorage questionnaireStorage = null)
        {
            var assignmentsService = new Mock<IAssignmentsService>();
            assignmentsService.Setup(x => x.GetAssignmentsReadyForWebInterview(It.IsAny<QuestionnaireIdentity>()))
                .Returns(new List<Assignment>());
            return new SampleWebInterviewService(
                assignments ?? assignmentsService.Object,
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
        }
    }
}