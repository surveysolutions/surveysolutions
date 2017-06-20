﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Tests.Unit.Applications.Headquarters.InterviewerApiTests
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

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(Id.g7, preFilled: true),
                    Create.Entity.GpsCoordinateQuestion(Id.g3, isPrefilled: true)
            ));

            this.assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.g1), 10, Id.g2, "int", summary);
            this.assignment.SetIdentifyingData(new List<IdentifyingAnswer>
            {
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Create.Identity(Id.g7)),
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Create.Identity(Id.g3)),
            });
            this.assignment.SetAnswers(new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(Id.g3),
                    Answer = TextAnswer.FromString("answer1")
                },
                new InterviewAnswer
                {
                    Identity = Create.Identity(Id.g7),
                    Answer = TextAnswer.FromString("answer2")
                },
                new InterviewAnswer
                {
                    Identity = Create.Identity(Id.g3),
                    Answer = GpsAnswer.FromGeoPosition(new GeoPosition(10.0, 20.0, 10, 10, DateTimeOffset.MaxValue))
                }
            });

            var service = new AssignmentViewFactory(null, null, answerSerializer);

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

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(Id.g7, preFilled: true),
                Create.Entity.GpsCoordinateQuestion(Id.g3, isPrefilled: true)
            ));

            this.assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.g1),
                    quantity: null, assigneeSupervisorId: Id.g2, responsibleName: "int", interviewSummary: summary);

            this.assignment.SetIdentifyingData(new List<IdentifyingAnswer>
            {
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Create.Identity(Id.g7)),
                IdentifyingAnswer.Create(assignment, questionnaire, "test", Create.Identity(Id.g3)),
            });
            this.assignment.SetAnswers(new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(Id.g3),
                    Answer = TextAnswer.FromString("answer1")
                },
                new InterviewAnswer
                {
                    Identity = Create.Identity(Id.g7),
                    Answer = TextAnswer.FromString("answer2")
                },
                new InterviewAnswer
                {
                    Identity = Create.Identity(Id.g3),
                    Answer = GpsAnswer.FromGeoPosition(new GeoPosition(10.0, 20.0, 10, 10, DateTimeOffset.MaxValue))
                }
            });

            var service = new AssignmentViewFactory(null, null, answerSerializer);

            // act
            this.assignmentApiDocument = service.MapAssignment(this.assignment);
        }
        
        [Test]
        public void should_map_questionnaire_quantity() =>
            Assert.That(this.assignmentApiDocument.Quantity, Is.Null);
    }
}
