using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerApiTests
{
    public class when_map_assignment_to_assignmentApiDocument_AssignmentViewFactoryTests
    {
        private Assignment assignment;
        private AssignmentApiDocument assignmentApiDocument;
        private readonly NewtonInterviewAnswerJsonSerializer answerSerializer = new NewtonInterviewAnswerJsonSerializer();

        [SetUp]
        public void Setup()
        {
            var summary = new HashSet<InterviewSummary>
            {
                new InterviewSummary(),
                new InterviewSummary()
            };

            var questionnaire = Abc.Create.Entity.PlainQuestionnaire(Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Abc.Create.Entity.TextQuestion(Id.g7, preFilled: true),
                    Abc.Create.Entity.GpsCoordinateQuestion(Id.g3, isPrefilled: true)
            ));

            this.assignment = Abc.Create.Entity.Assignment(1, Abc.Create.Entity.QuestionnaireIdentity(Id.g1), 10, Id.g2, "int", summary);
            this.assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Abc.Create.Identity(Id.g7)),
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Abc.Create.Identity(Id.g3)),
            };
            this.assignment.Answers = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Abc.Create.Identity(Id.g3),
                    Answer = TextAnswer.FromString("answer1")
                },
                new InterviewAnswer
                {
                    Identity = Abc.Create.Identity(Id.g7),
                    Answer = TextAnswer.FromString("answer2")
                },
                new InterviewAnswer
                {
                    Identity = Abc.Create.Identity(Id.g3),
                    Answer = GpsAnswer.FromGeoPosition(new GeoPosition(10.0, 20.0, 10, 10, DateTimeOffset.MaxValue))
                }
            };

            var service = new AssignmentsService(null, answerSerializer, null, null);

            // act
            this.assignmentApiDocument = service.MapAssignment(this.assignment);
        }

        [Test]
        public void should_map_questionnaire_id() =>
            Assert.That(this.assignmentApiDocument.QuestionnaireId, Is.EqualTo(this.assignment.QuestionnaireId));

        [Test]
        public void should_map_questionnaire_quantity() =>
            Assert.That(this.assignmentApiDocument.Quantity, Is.EqualTo(assignment.Quantity - assignment.InterviewSummaries.Count));

        [Test]
        public void should_map_questionnaire_location_to_gps_question()
        {
            Assert.That(this.assignmentApiDocument.LocationLatitude, Is.EqualTo(10.0));
            Assert.That(this.assignmentApiDocument.LocationLongitude, Is.EqualTo(20.0));
            Assert.That(this.assignmentApiDocument.LocationQuestionId, Is.EqualTo(Id.g3));
        }

        [Test]
        public void should_map_questionnaire_answers()
        {
            Assert.That(this.assignmentApiDocument.Answers,
                Has.All.Matches<AssignmentApiDocument.InterviewSerializedAnswer>(f =>
                    this.assignment.Answers.Any(e =>
                        e.Identity == f.Identity
                    )
                ));
        }


        [Test]
        public void should_map_creation_date()
        {
            this.assignmentApiDocument.CreatedAtUtc.Should().Be(this.assignment.CreatedAtUtc);
        }
    }

    public class when_map_assignment_to_assignmentApiDocument_with_null_quantity
    {
        private Assignment assignment;
        private AssignmentApiDocument assignmentApiDocument;
        private readonly NewtonInterviewAnswerJsonSerializer answerSerializer = new NewtonInterviewAnswerJsonSerializer();

        [SetUp]
        public void Setup()
        {
            var summary = new HashSet<InterviewSummary>
            {
                new InterviewSummary(),
                new InterviewSummary()
            };

            var questionnaire = Abc.Create.Entity.PlainQuestionnaire(Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(
                Abc.Create.Entity.TextQuestion(Id.g7, preFilled: true),
                Abc.Create.Entity.GpsCoordinateQuestion(Id.g3, isPrefilled: true)
            ));

            this.assignment = Abc.Create.Entity.Assignment(1, Abc.Create.Entity.QuestionnaireIdentity(Id.g1),
                    quantity: null, assigneeSupervisorId: Id.g2, responsibleName: "int", interviewSummary: summary);

            this.assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Abc.Create.Identity(Id.g7)),
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Abc.Create.Identity(Id.g3)),
            };
            this.assignment.Answers = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Abc.Create.Identity(Id.g3),
                    Answer = TextAnswer.FromString("answer1")
                },
                new InterviewAnswer
                {
                    Identity = Abc.Create.Identity(Id.g7),
                    Answer = TextAnswer.FromString("answer2")
                },
                new InterviewAnswer
                {
                    Identity = Abc.Create.Identity(Id.g3),
                    Answer = GpsAnswer.FromGeoPosition(new GeoPosition(10.0, 20.0, 10, 10, DateTimeOffset.MaxValue))
                }
            };

            var service = new AssignmentsService(null, answerSerializer, null, null);

            // act
            this.assignmentApiDocument = service.MapAssignment(this.assignment);
        }
        
        [Test]
        public void should_map_questionnaire_quantity() =>
            Assert.That(this.assignmentApiDocument.Quantity, Is.Null);
    }
}
