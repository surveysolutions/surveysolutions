using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Integration
{
    internal partial class InterviewFactoryTests
    {
        [Test]
        public void when_getting_question_ids_of_answered_gps_questions_by_questionnaire_id()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);

            var expectedGpsAnswers = new[]
            {
                new
                {
                    InterviewId = interviewId,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = interviewId,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(2)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in expectedGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = questionnaireId.ToString()

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(expectedGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in expectedGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var allGpsQuestionIds = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetAnsweredGpsQuestionIdsByQuestionnaire(questionnaireId));

            //assert
            Assert.That(allGpsQuestionIds.Length, Is.EqualTo(3));
            Assert.That(allGpsQuestionIds, Is.EquivalentTo(expectedGpsAnswers.Select(x => x.QuestionId.Id)));
        }

        [Test]
        public void when_getting_question_ids_of_answered_gps_questions_by_questionnaire_id_and_teamlead_id()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var supervisorid = Guid.Parse("11111111111111111111111111111111");

            var expectedGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    TeamLeadId = supervisorid,
                    QuestionnaireId = questionnaireId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(2)),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    TeamLeadId = supervisorid,
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 11223),
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in expectedGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString(),
                        TeamLeadId = gpsAnswer.TeamLeadId

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(expectedGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in expectedGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var allGpsQuestionIds = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetAnsweredGpsQuestionIdsByQuestionnaireAndSupervisor(questionnaireId, supervisorid));

            //assert
            Assert.That(allGpsQuestionIds.Length, Is.EqualTo(1));
            Assert.That(allGpsQuestionIds,
                Is.EquivalentTo(expectedGpsAnswers
                    .Where(x => x.QuestionnaireId == questionnaireId && x.TeamLeadId == supervisorid)
                    .Select(x => x.QuestionId.Id)));
        }

        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        public void when_getting_question_ids_of_answered_gps_questions_by_questionnaire_id_and_teamlead_id_and_interview_status_not_allowed(InterviewStatus status)
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var interviewId =  Guid.Parse("33333333333333333333333333333333");
            var supervisorid = Guid.Parse("11111111111111111111111111111111");
            var gpsQuestionIdentity = InterviewStateIdentity.Create(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty);

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = interviewId,
                        Status = status,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = questionnaireId.ToString(),
                        TeamLeadId = supervisorid

                    }, interviewId.FormatGuid());
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.GpsCoordinateQuestion(gpsQuestionIdentity.Id));

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<GeoPosition>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var interviewState = Create.Entity.InterviewState(interviewId);
                interviewState.Answers = new Dictionary<InterviewStateIdentity, InterviewStateAnswer>
                {
                    {
                        gpsQuestionIdentity,
                        new InterviewStateAnswer
                        {
                            Id = gpsQuestionIdentity.Id,
                            RosterVector = gpsQuestionIdentity.RosterVector,
                            AsGps = entitySerializer.Serialize(new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now})
                        }
                    }
                };

                factory.Save(interviewState);
            });

            //act
            var allGpsQuestionIds = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                factory.GetAnsweredGpsQuestionIdsByQuestionnaireAndSupervisor(questionnaireId, supervisorid));

            //assert
            Assert.IsEmpty(allGpsQuestionIds);
        }

        [Test]
        public void when_getting_questionnaire_ids_by_answered_gps_questions()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);

            var expectedGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in expectedGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString()

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(expectedGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.plainTransactionManager.GetSession().Connection.Execute($"DELETE FROM {InterviewsTableName}");

                foreach (var groupedInterviews in expectedGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var questionnaireIdentities = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetQuestionnairesWithAnsweredGpsQuestions());

            //assert
            Assert.That(questionnaireIdentities.Length, Is.EqualTo(2));
            Assert.That(questionnaireIdentities, Is.EquivalentTo(expectedGpsAnswers.Select(x => x.QuestionnaireId.ToString()).Distinct()));
        }

        [Test]
        public void when_getting_questionnaire_ids_by_teamlead_id_and_answered_gps_questions()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var supervisorId = Guid.Parse("11111111111111111111111111111111");

            var expectedGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    TeamLeadId = supervisorId,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    TeamLeadId = Guid.NewGuid(),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    TeamLeadId = supervisorId,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in expectedGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString(),
                        TeamLeadId = gpsAnswer.TeamLeadId

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(expectedGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.plainTransactionManager.GetSession().Connection.Execute($"DELETE FROM {InterviewsTableName}");

                foreach (var groupedInterviews in expectedGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var questionnaireIdentities = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetQuestionnairesWithAnsweredGpsQuestionsBySupervisor(supervisorId));

            //assert
            Assert.That(questionnaireIdentities.Length, Is.EqualTo(1));
            Assert.That(questionnaireIdentities[0], Is.EqualTo(questionnaireId.ToString()));
        }

        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        public void when_getting_questionnaire_ids_by_teamlead_id_and_answered_gps_questions_and_interview_status_not_allowed(InterviewStatus status)
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var supervisorid = Guid.Parse("11111111111111111111111111111111");
            var interviewid =  Guid.Parse("22222222222222222222222222222222");
            var gpsQuestionIdentity = InterviewStateIdentity.Create(Guid.Parse("33333333333333333333333333333333"), RosterVector.Empty);

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                interviewSummaryRepository.Store(new InterviewSummary
                {
                    InterviewId = interviewid,
                    Status = status,
                    ReceivedByInterviewer = false,
                    QuestionnaireIdentity = questionnaireId.ToString(),
                    TeamLeadId = supervisorid

                }, interviewid.FormatGuid());
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(gpsQuestionIdentity.Id));

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.plainTransactionManager.GetSession().Connection.Execute($"DELETE FROM {InterviewsTableName}");

                var interviewState = Create.Entity.InterviewState(interviewid);
                interviewState.Answers = new Dictionary<InterviewStateIdentity, InterviewStateAnswer>
                {
                    {
                        gpsQuestionIdentity,
                        new InterviewStateAnswer
                        {
                            Id = gpsQuestionIdentity.Id,
                            RosterVector = gpsQuestionIdentity.RosterVector,
                            AsGps = entitySerializer.Serialize(new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now})
                        }
                    }
                };

                factory.Save(interviewState);
            });

            //act
            var questionnaireIdentities = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetQuestionnairesWithAnsweredGpsQuestionsBySupervisor(supervisorid));

            //assert
            Assert.IsEmpty(questionnaireIdentities);
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var allGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString()

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(allGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in allGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(
                () => factory.GetGpsAnswersByQuestionIdAndQuestionnaire(questionnaireId, questionId.Id, 10, 90, -90, 180, -180));

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(2));
            Assert.That(gpsAnswers, Is.EquivalentTo(allGpsAnswers
                    .Where(x => x.QuestionId == questionId && x.QuestionnaireId == questionnaireId)
                    .Select(x => new InterviewGpsAnswer
                    {
                        InterviewId = x.InterviewId,
                        Longitude = x.Answer.Longitude,
                        Latitude = x.Answer.Latitude
                    })));
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_teamlead_id()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var teamleadid = Guid.Parse("11111111111111111111111111111111");

            var allGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    TeamLeadId = teamleadid,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    TeamLeadId = teamleadid,
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    TeamLeadId = Guid.NewGuid(),
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString(),
                        TeamLeadId = gpsAnswer.TeamLeadId

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(allGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in allGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(
                () => factory.GetGpsAnswersByQuestionIdAndQuestionnaireAndSupervisor(questionnaireId, questionId.Id, 10, 90, -90, 180, -180, teamleadid));

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));
            Assert.That(gpsAnswers, Is.EquivalentTo(allGpsAnswers
                    .Where(x => x.QuestionId == questionId && x.QuestionnaireId == questionnaireId && x.TeamLeadId == teamleadid)
                    .Select(x => new InterviewGpsAnswer
                    {
                        InterviewId = x.InterviewId,
                        Longitude = x.Answer.Longitude,
                        Latitude = x.Answer.Latitude
                    })));
        }

        [TestCase(InterviewStatus.ApprovedBySupervisor)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters)]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_teamlead_id_and_interview_status_not_allowed(InterviewStatus status)
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var teamleadid = Guid.Parse("11111111111111111111111111111111");
            var interviewid = Guid.Parse("22222222222222222222222222222222");

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                interviewSummaryRepository.Store(new InterviewSummary
                {
                    InterviewId = interviewid,
                    Status = status,
                    ReceivedByInterviewer = false,
                    QuestionnaireIdentity = questionnaireId.ToString(),
                    TeamLeadId = teamleadid

                }, interviewid.FormatGuid());
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.GpsCoordinateQuestion(questionId.Id));

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {

                var interviewState = Create.Entity.InterviewState(interviewid);
                interviewState.Answers = new Dictionary<InterviewStateIdentity, InterviewStateAnswer>
                {
                    {
                        questionId,
                        new InterviewStateAnswer
                        {
                            Id = questionId.Id,
                            RosterVector = questionId.RosterVector,
                            AsGps = entitySerializer.Serialize(new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now})
                        }
                    }
                };

                factory.Save(interviewState);

            });

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(
                () => factory.GetGpsAnswersByQuestionIdAndQuestionnaireAndSupervisor(questionnaireId, questionId.Id, 10, 90, -90, 180, -180, teamleadid));

            //assert
            Assert.IsEmpty(gpsAnswers);
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_west_latitude()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var allGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString()

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(allGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in allGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId, x => new InterviewStateAnswer
                    {
                        Id = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector,
                        AsGps = entitySerializer.Serialize(x.Answer)
                    });

                    factory.Save(interviewState);
                }
            });

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetGpsAnswersByQuestionIdAndQuestionnaire(questionnaireId, questionId.Id, 10, 90, 2, 180, -180));

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
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_answers_count()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

            var allGpsAnswers = new[]
            {
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    QuestionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = questionId,
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = gpsAnswer.InterviewId,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = gpsAnswer.QuestionnaireId.ToString()

                    }, gpsAnswer.InterviewId.FormatGuid());
                }
            });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(allGpsAnswers
                .Select(x => Create.Entity.GpsCoordinateQuestion(x.QuestionId.Id)).OfType<IComposite>().ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in allGpsAnswers.GroupBy(x => x.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId,
                        x => new InterviewStateAnswer
                        {
                            Id = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector,
                            AsGps = entitySerializer.Serialize(x.Answer)
                        });

                    factory.Save(interviewState);
                }
            });

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetGpsAnswersByQuestionIdAndQuestionnaire(questionnaireId, questionId.Id, 1, 90, -90, 180, -180));

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));
        }
    }
}