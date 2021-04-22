using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
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
            byte[] outputBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(outputBytes);


            Assert.That(reader[0].Length, Is.EqualTo(4));
            Assert.That(reader[0][0], Is.EqualTo("assignment__link"));
            Assert.That(reader[0][1], Is.EqualTo("assignment__id"));
            Assert.That(reader[0][2], Is.EqualTo("assignment__email"));
            Assert.That(reader[0][3], Is.EqualTo("assignment__password"));

            Assert.That(reader.Count, Is.EqualTo(1));
        }

        [Test]
        public void When_generating_for_questionnaire_without_prefilled_questions_Should_ouput_links()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();

            var assignmentId = 5;
            var assignment = Create.Entity.Assignment(id: assignmentId, questionnaireIdentity: questionnaireId,
                assigneeSupervisorId: Guid.NewGuid(), email: "email@super.com", password: "PASSWORD", webMode:true);
            var invitations = Create.Service.InvitationService(Create.Entity.Invitation(48, token: "AAAAAAAA", assignment: assignment));
            var service = this.GetService(invitations);

            // Act
            byte[] outputBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(outputBytes);

            Assert.That(reader[0].Length, Is.EqualTo(4));
            Assert.That(reader[0][0], Is.EqualTo("assignment__link"));
            Assert.That(reader[0][1], Is.EqualTo("assignment__id"));
            Assert.That(reader[0][2], Is.EqualTo("assignment__email"));
            Assert.That(reader[0][3], Is.EqualTo("assignment__password"));

            Assert.That(reader.Count, Is.EqualTo(2));
            Assert.That(reader[1][0], Is.EqualTo($"http://baseurl/AAAAAAAA/Start"));
            Assert.That(reader[1][1], Is.EqualTo($"{assignmentId}"));
            Assert.That(reader[1][2], Is.EqualTo("email@super.com"));
            Assert.That(reader[1][3], Is.EqualTo("PASSWORD"));
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
            assignment1.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment1, Create.Identity(Id.g1),
                answer: "1"));

            var assignment2 = Create.Entity.Assignment(questionnaireIdentity: questionnaireId);
            assignment2.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment2, Create.Identity(Id.g2),
                answer: "2"));

            var assignment3 = Create.Entity.Assignment(questionnaireIdentity: questionnaireId);
            assignment3.IdentifyingData.Add(Create.Entity.IdentifyingAnswer(assignment3, Create.Identity(Id.g3),
                answer: "3"));

            var invitations = new Mock<IInvitationService>();
            invitations.Setup(x => x.GetInvitationsToExport(questionnaireId))
                .Returns(new List<Invitation>
                {
                    Create.Entity.Invitation(48, token: "AAAAAAAA", assignment: assignment1),
                    Create.Entity.Invitation(49, token: "BBBBBBBB", assignment: assignment2),
                    Create.Entity.Invitation(47, token: "CCCCCCCC", assignment: assignment3)
                });

            var service = this.GetService(invitations: invitations.Object,
                SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire));

            // Act
            byte[] outputBytes = service.Generate(questionnaireId, "http://baseurl");

            // Assert
            var reader = GetCsvReader(outputBytes);

            Assert.That(reader[0], Is.EquivalentTo(new[] {"assignment__link", "assignment__id", "assignment__email", "assignment__password", "q1", "q2", "q3"}));
            Assert.That(reader[1][4], Is.EqualTo($"1"));
            Assert.That(reader[2][5], Is.EqualTo($"2"));
            Assert.That(reader[3][6], Is.EqualTo($"3"));
        }

        private static List<string[]> GetCsvReader(byte[] outputBytes)
        {
            var result = new List<string[]>();
            var streamReader = new StreamReader(new MemoryStream(outputBytes), Encoding.UTF8);
            var reader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreQuotes = false,
                Delimiter = "\t",
                MissingFieldFound = null,
            });

            reader.Read();
            reader.ReadHeader();

            var columnsCount = reader.Context.HeaderRecord.Length;
            result.Add(reader.Context.HeaderRecord);

            while (reader.Read())
            {
                var row = new string[columnsCount];
                for (int i = 0; i < columnsCount; i++)
                {
                    if (reader.TryGetField<string>(i, out string value))
                    {
                        row[i] = value;
                    }
                    else
                    {
                        row[i] = null;
                    }
                }
                result.Add(row);    
            }
            
            return result;
        }

        private SampleWebInterviewService GetService(IInvitationService invitations = null,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            var invitationServiceMock = new Mock<IInvitationService>();
            invitationServiceMock.Setup(x => x.GetInvitationsToExport(It.IsAny<QuestionnaireIdentity>()))
                .Returns(new List<Invitation>());

            return new SampleWebInterviewService(questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                invitations ?? invitationServiceMock.Object);
        }
    }
}
