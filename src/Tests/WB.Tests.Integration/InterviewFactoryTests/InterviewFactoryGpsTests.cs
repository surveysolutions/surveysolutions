using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using Supercluster;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
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
        private Identity gpsQuestionId;
        private Identity anotherGpsQuestionId;
        private InterviewFactory factory;

        [SetUp]
        public void Setup()
        {
            //arrange
            this.questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            this.otherQuestionnaireId = new QuestionnaireIdentity(questionnaireId.QuestionnaireId, 777);

            this.gpsQuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            this.anotherGpsQuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId.QuestionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.GpsCoordinateQuestion(gpsQuestionId.Id, "gps"),
                    Create.Entity.GpsCoordinateQuestion(anotherGpsQuestionId.Id, "gps2")
                });

            this.factory = CreateInterviewFactory();

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);
            PrepareQuestionnaire(questionnaire, otherQuestionnaireId.Version);
        }

        [Test]
        public void when_HasAnyGpsAnswerForInterviewer_and_user_has_interview_with_answered_gps_question_should_be_true()
        {
            //arrange
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var answers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    ResponsibleId = interviewerId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(answers);

            //act
            var hasAnswers = factory.HasAnyGpsAnswerForInterviewer(interviewerId);

            //assert
            Assert.That(hasAnswers, Is.True);
        }

        [Test]
        public void when_HasAnyGpsAnswerForInterviewer_and_user_hasnt_interviews_with_answered_gps_questions_should_be_false()
        {
            //arrange
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var answers = new[]
            {
                new GpsAnswer
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = gpsQuestionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(answers);

            //act
            var hasAnswers = factory.HasAnyGpsAnswerForInterviewer(interviewerId);

            //assert
            Assert.That(hasAnswers, Is.False);
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_all_versions()
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
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            PrepareAnswers(answers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId.QuestionnaireId, null, "gps", 10,
                GeoBounds.Open, null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(3));
            Assert.That(gpsAnswers, Is.EquivalentTo(answers
                .Where(x => x.QuestionId == gpsQuestionId)
                .Select(x => new InterviewGpsAnswer
                {
                    InterviewId = x.InterviewId,
                    Longitude = x.Answer.Longitude,
                    Latitude = x.Answer.Latitude
                })));
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
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 10,
                GeoBounds.Open, null);

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

            var expectedGpsAnswer = new GpsAnswer
            {
                InterviewId = Guid.NewGuid(),
                QuestionnaireId = questionnaireId,
                QuestionId = gpsQuestionId,
                Answer = new GeoPosition
                    {Longitude = 4, Latitude = 4, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
            };

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
                expectedGpsAnswer
            };

            PrepareAnswers(allGpsAnswers);

            //act
            var gpsAnswers = factory.GetGpsAnswers(
                questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 10, new GeoBounds(3, 3, 5, 5), null);

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));
            Assert.AreEqual(gpsAnswers[0],
                new InterviewGpsAnswer
                {
                    InterviewId = expectedGpsAnswer.InterviewId,
                    Longitude = expectedGpsAnswer.Answer.Longitude,
                    Latitude = expectedGpsAnswer.Answer.Latitude
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
                questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 10, GeoBounds.Open, null);

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
            var gpsAnswers = factory.GetGpsAnswers(
                questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 10, GeoBounds.Open, null);

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
                questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 1, GeoBounds.Open, null);

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
                questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 2, GeoBounds.Open, supervisorId);

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
            var gpsAnswers = factory.GetGpsAnswers(questionnaireId.QuestionnaireId, questionnaireId.Version, "gps", 2, GeoBounds.Open, supervisorId);

            //assert
            Assert.IsEmpty(gpsAnswers);
        }

        protected class GpsAnswer
        {
            public bool IsEnabled { get; set; } = true;
            public Guid InterviewId { get; set; }
            public Guid? TeamLeadId { get; set; }
            public Guid? ResponsibleId { get; set; }
            public QuestionnaireIdentity QuestionnaireId { get; set; }
            public Identity QuestionId { get; set; }
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
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString(),
                        QuestionnaireId = gpsAnswer.QuestionnaireId.QuestionnaireId,
                        QuestionnaireVersion = gpsAnswer.QuestionnaireId.Version,
                        ResponsibleId = gpsAnswer.ResponsibleId ?? Guid.NewGuid()
                    }, gpsAnswer.InterviewId.FormatGuid());
                    factory.SaveGeoLocation(gpsAnswer.InterviewId, gpsAnswer.QuestionId, gpsAnswer.Answer.Latitude,
                        gpsAnswer.Answer.Longitude, gpsAnswer.Answer.Timestamp);
                    factory.EnableGeoLocationAnswers(gpsAnswer.InterviewId, new[] {gpsAnswer.QuestionId},
                        gpsAnswer.IsEnabled);
                }
                unitOfWork.AcceptChanges();
            }
        }
    }
}
