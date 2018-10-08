using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewFactoryTests
{
    internal class InterviewFactoryGpsTests : InterviewFactorySpecification
    {
        private QuestionnaireIdentity questionnaireId;
        private QuestionnaireIdentity otherQuestionnaireId;
        private InterviewStateIdentity gpsQuestionId;
        private InterviewStateIdentity anotherGpsQuestionId;
        private InterviewFactory factory;

        [SetUp]
        public void Setup()
        {
            //arrange
            this.questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            this.otherQuestionnaireId = new QuestionnaireIdentity(questionnaireId.QuestionnaireId, 777);

            this.gpsQuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            this.anotherGpsQuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId.QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.GpsCoordinateQuestion(gpsQuestionId.Id),
                    Create.Entity.GpsCoordinateQuestion(anotherGpsQuestionId.Id)
                });

            this.factory = CreateInterviewFactory();

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);
            PrepareQuestionnaire(questionnaire, otherQuestionnaireId.Version);
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id()
        {
            //arrange
            var answers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = anotherGpsQuestionId,
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = otherQuestionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(answers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId, gpsQuestionId.Id, 10, 90, -90, 180, -180, null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(2));
            Assert.That(gpsAnswers, Is.EquivalentTo(answers
                .Where(x => x.QuestionId == gpsQuestionId && x.QuestionnaireId == questionnaireId)
                .Select(x => new InterviewGpsAnswer
                {
                    InterviewId = x.InterviewId,
                    Longitude = x.Answer.Longitude,
                    Latitude = x.Answer.Latitude
                })));
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_west_latitude()
        {
            //arrange

            var allGpsAnswers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = otherQuestionnaireId,
                    QuestionId = anotherGpsQuestionId,
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId, gpsQuestionId.Id, 10, 90, 2, 180, -180, null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));
            Assert.AreEqual(gpsAnswers[0],
                new InterviewGpsAnswer
                {
                    InterviewId = allGpsAnswers[2].InterviewId,
                    Longitude = allGpsAnswers[2].Answer.Longitude,
                    Latitude = allGpsAnswers[2].Answer.Latitude
                });
        }

        [Test]
        public void when_getting_gps_answers_should_return_only_enabled()
        {
            //arrange

            var allGpsAnswers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    IsEnabled = false,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(
                    questionnaireId, gpsQuestionId.Id, 10, 90, 0, 180, -180, null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));

            Assert.That(gpsAnswers[0], Has.Property(nameof(InterviewGpsAnswer.InterviewId)).EqualTo(allGpsAnswers[0].InterviewId));
            Assert.That(gpsAnswers[0], Has.Property(nameof(InterviewGpsAnswer.Longitude)).EqualTo(allGpsAnswers[0].Answer.Longitude));
            Assert.That(gpsAnswers[0], Has.Property(nameof(InterviewGpsAnswer.Latitude)).EqualTo(allGpsAnswers[0].Answer.Latitude));
        }

        [Test]
        public void when_getting_gps_answers_without_filter_by_supervisor_should_return_all_statuses_and_teams()
        {
            //arrange

            var allGpsAnswers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    TeamLeadId = Guid.NewGuid(),
                    InterviewStatus = InterviewStatus.Completed,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    InterviewStatus = InterviewStatus.ApprovedByHeadquarters,
                    IsEnabled = true,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    InterviewStatus = InterviewStatus.ApprovedBySupervisor,
                    IsEnabled = true,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    TeamLeadId = Guid.NewGuid(),
                    InterviewStatus = InterviewStatus.InterviewerAssigned,
                    IsEnabled = true,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId, gpsQuestionId.Id, 10, 90, 0, 180, -180, null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(allGpsAnswers.Length));
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_answers_count()
        {
            //arrange
            var allGpsAnswers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = otherQuestionnaireId,
                    QuestionId = anotherGpsQuestionId,
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(
                    questionnaireId, gpsQuestionId.Id, 1, 90, -90, 180, -180, null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_supervisorid()
        {
            var supervisorId = Id.g1;
            //arrange
            var allGpsAnswers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = supervisorId,
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1,
                        Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId =  supervisorId,
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2,
                        Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionnaireId = otherQuestionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = supervisorId,
                    QuestionnaireId = otherQuestionnaireId,
                    QuestionId = anotherGpsQuestionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(
                    questionnaireId, gpsQuestionId.Id, 2, 90, -90, 180, -180, supervisorId);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(2));

            Assert.That(gpsAnswers,
                Has.One.Property(nameof(InterviewGpsAnswer.InterviewId)).EqualTo(allGpsAnswers[0].InterviewId));
            Assert.That(gpsAnswers,
                Has.One.Property(nameof(InterviewGpsAnswer.InterviewId)).EqualTo(allGpsAnswers[1].InterviewId));

            Assert.That(gpsAnswers,
                Has.None.Property(nameof(InterviewGpsAnswer.InterviewId)).EqualTo(allGpsAnswers[2].InterviewId));
        }


        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        public void when_getting_gps_answers_by_teamlead_id_and_interview_status_not_allowed_should_not_return_answers(InterviewStatus status)
        {
            var supervisorId = Id.g1;
            //arrange
            var allGpsAnswers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = supervisorId,
                    InterviewStatus = status,
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1,
                        Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId =  supervisorId,
                    QuestionnaireId = questionnaireId,
                    InterviewStatus = status,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2,
                        Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    InterviewStatus = status,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId, gpsQuestionId.Id, 2, 90, -90, 180, -180, supervisorId);

            //assert
            Assert.IsEmpty(gpsAnswers);
        }

        protected class GpsAnswer
        {
            public bool IsEnabled { get; set; } = true;
            public Guid InterviewId { get; set; }
            public Guid? TeamLeadId { get; set; }
            public QuestionnaireIdentity QuestionnaireId { get; set; }
            public InterviewStateIdentity QuestionId { get; set; }
            public GeoPosition Answer { get; set; }
            public InterviewStatus? InterviewStatus { get; set; }
        }

        protected void PrepareAnswers(ICollection<GpsAnswer> answers)
        {

            using (var unitOfWork = IntegrationCreate.UnitOfWork(sessionFactory))
            {
                var interviewSummaryRepositoryLocal = new PostgreReadSideStorage<InterviewSummary>(unitOfWork, Mock.Of<ILogger>(), Mock.Of<IServiceLocator>());

                foreach (var gpsAnswer in answers)
                {
                    interviewSummaryRepositoryLocal.Store(new InterviewSummary
                    {
                        Status = gpsAnswer.InterviewStatus ?? InterviewStatus.Completed,
                        TeamLeadId = gpsAnswer.TeamLeadId ?? Guid.Empty,
                        InterviewId = gpsAnswer.InterviewId,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString()
                    }, gpsAnswer.InterviewId.FormatGuid());
                }

                unitOfWork.AcceptChanges();
            }
            

            foreach (var groupedInterviews in answers.GroupBy(x => x.InterviewId))
            {
                var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                {
                    Id = x.QuestionId.Id,
                    RosterVector = x.QuestionId.RosterVector,
                    AsGps = x.Answer
                });
                interviewState.Enablement = groupedInterviews.ToDictionary(x => x.QuestionId, x => x.IsEnabled);
                factory.Save(interviewState);
            }
        }
    }
}
