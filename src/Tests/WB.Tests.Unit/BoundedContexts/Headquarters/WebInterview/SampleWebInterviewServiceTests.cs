using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Core.SharedKernels.DataCollection;
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
            Assert.That(reader.Context.HeaderRecord[0], Is.EqualTo("assignment__link"));
            Assert.That(reader.Context.HeaderRecord[1], Is.EqualTo("assignment__id"));

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
            Assert.That(reader.Context.HeaderRecord[0], Is.EqualTo("assignment__link"));
            Assert.That(reader.Context.HeaderRecord[1], Is.EqualTo("assignment__id"));

            Assert.That(reader.GetField(0), Is.EqualTo($"http://baseurl/{assignmentId}/Start"));
            Assert.That(reader.GetField(1), Is.EqualTo($"{assignmentId}"));
        }

        [Test(Description = "KP-11812")]
        public void when_generating_for_interviews_with_partially_filled_prefilled_questions()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity(Id.gA);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: Id.g1, variable: "q1", preFilled: true),
                    Create.Entity.TextQuestion(questionId: Id.g2, variable: "q2", preFilled: true),
                    Create.Entity.TextQuestion(questionId: Id.g3, variable: "q3", preFilled: true)
                }
            );

            var assignment1 = Create.Entity.Assignment(questionnaireIdentity: questionnaireId);
            assignment1.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment1, Create.Identity(Id.g1), answer: "1"));

            var assignment2 = Create.Entity.Assignment(questionnaireIdentity: questionnaireId);
            assignment2.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment2, Create.Identity(Id.g2), answer: "2"));

            var assignment3 = Create.Entity.Assignment(questionnaireIdentity: questionnaireId);
            assignment3.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment3, Create.Identity(Id.g3), answer: "3"));

            var assignments = new Mock<IAssignmentsService>();
            assignments.Setup(x => x.GetAssignmentsReadyForWebInterview(questionnaireId))
                .Returns(new List<Assignment>
                {
                    assignment1, assignment2, assignment3
                });

            var service = this.GetService(assignments: assignments.Object, Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire));

            // act
            // Act
            byte[] ouptutBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(ouptutBytes);

            reader.Read();
            reader.ReadHeader();

            Assert.That(reader.Context.HeaderRecord, Is.EquivalentTo(new []{"assignment__link","assignment__id", "q1", "q2", "q3"}));

            reader.Read();
            Assert.That(reader.GetField(2), Is.EqualTo($"1"));

            reader.Read();
            
            Assert.That(reader.GetField(3), Is.EqualTo($"2"));

            reader.Read();
            Assert.That(reader.GetField(4), Is.EqualTo($"3"));
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
