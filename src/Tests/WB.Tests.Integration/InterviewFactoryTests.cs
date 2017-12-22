using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Dapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryTests
    {
        private const string InterviewsTableName = "readside.interviews";

        private PlainPostgresTransactionManager plainTransactionManager;
        private string connectionString;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.connectionString = DatabaseTestInitializer.InitializeDb(DbType.ReadSide);

            var sessionFactory = IntegrationCreate.SessionFactory(this.connectionString,
                new List<Type>
                {
                    typeof(InterviewSummaryMap),
                    typeof(QuestionAnswerMap),
                    typeof(TimeSpanBetweenStatusesMap)
                }, true, PostgresReadSideModule.ReadSideSchemaName);

            this.plainTransactionManager = new PlainPostgresTransactionManager(sessionFactory);

            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<int[][]>>(new EntitySerializer<int[][]>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<GeoPosition>>(new EntitySerializer<GeoPosition>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<InterviewTextListAnswer[]>>(new EntitySerializer<InterviewTextListAnswer[]>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<AnsweredYesNoOption[]>>(new EntitySerializer<AnsweredYesNoOption[]>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<AudioAnswer>>(new EntitySerializer<AudioAnswer>());
            Abc.Setup.InstanceToMockedServiceLocator<IEntitySerializer<Area>>(new EntitySerializer<Area>());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.plainTransactionManager.Dispose();
            DatabaseTestInitializer.DropDb(this.connectionString);
        }

        [Test]
        public void when_getting_flagged_question_ids()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1))
            };

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ReceivedByInterviewer = false
            }, interviewId.FormatGuid());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);
            foreach (var questionIdentity in questionIdentities)
                this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.SetFlagToQuestion(interviewId, questionIdentity, true));

            //act
            var flaggedIdentites = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetFlaggedQuestionIds(interviewId));

            //assert
            Assert.AreEqual(flaggedIdentites.Length, 2);
            Assert.That(flaggedIdentites, Is.EquivalentTo(questionIdentities));
        }

        [Test]
        public void when_set_flag_to_question()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionIdentity = Identity.Create(Guid.NewGuid(), Create.RosterVector());

            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ReceivedByInterviewer = false
            }, interviewId.FormatGuid());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.SetFlagToQuestion(interviewId, questionIdentity, true));
            
            //assert
            var flaggedIdentities = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetFlaggedQuestionIds(interviewId));

            Assert.AreEqual(flaggedIdentities.Length, 1);
            Assert.AreEqual(flaggedIdentities[0].Id, questionIdentity.Id);
            Assert.AreEqual(flaggedIdentities[0].RosterVector, questionIdentity.RosterVector);
        }

        [Test]
        public void when_remove_flag_from_question()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionIdentity = Identity.Create(Guid.NewGuid(), Create.RosterVector());

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.SetFlagToQuestion(interviewId, questionIdentity, false));

            //assert
            var flaggedIdentities = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetFlaggedQuestionIds(interviewId));

            Assert.That(flaggedIdentities, Is.Empty);
        }

        [Test]
        public void when_remove_interview()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var interviewEntityIds = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4)),
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(new []
                {
                    new InterviewEntity{ InterviewId = interviewId, Identity = interviewEntityIds[0], EntityType = EntityType.Question, IsEnabled = true},
                    new InterviewEntity{ InterviewId = interviewId, Identity = interviewEntityIds[1], EntityType = EntityType.Section, IsEnabled = true},
                    new InterviewEntity{ InterviewId = interviewId, Identity = interviewEntityIds[2], EntityType = EntityType.StaticText, IsEnabled = true},
                    new InterviewEntity{ InterviewId = interviewId, Identity = interviewEntityIds[3], EntityType = EntityType.Variable, IsEnabled = true},

                }, new InterviewEntity[0]);
            });

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.RemoveInterview(interviewId));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities, Is.Empty);
        }

        [Test]
        public void when_make_question_readonly()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var readOnlyQuestions = new []
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(readOnlyQuestions.Select(x => new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = x,
                    EntityType = EntityType.Question,
                    IsEnabled = true,
                    IsReadonly = true
                }).ToArray(), new InterviewEntity[0]);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);
            
            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(readOnlyQuestions, Is.EquivalentTo(interviewEntities.Select(x=>x.Identity)));
        }

        [TestCase(EntityType.StaticText)]
        [TestCase(EntityType.Question)]
        [TestCase(EntityType.Variable)]
        public void when_enable_entities(EntityType entityType)
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var enabledEntities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(enabledEntities.Select(x => new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = x,
                    EntityType = entityType,
                    IsEnabled = true
                }).ToArray(), new InterviewEntity[0]);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(enabledEntities, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
        }

        [TestCase(EntityType.StaticText)]
        [TestCase(EntityType.Question)]
        [TestCase(EntityType.Variable)]
        public void when_make_entities_valid(EntityType entityType)
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var validEntities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(validEntities.Select(x => new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = x,
                    EntityType = entityType,
                    IsEnabled = true,
                    InvalidValidations = null
                }).ToArray(), new InterviewEntity[0]);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(validEntities, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
        }

        [TestCase(EntityType.StaticText)]
        [TestCase(EntityType.Question)]
        public void when_make_entities_invalid(EntityType entityType)
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var invalidEntities = new Dictionary<Identity, int[]>
            {
                {Identity.Create(Guid.NewGuid(), Create.RosterVector()), new[] {1}},
                {Identity.Create(Guid.NewGuid(), Create.RosterVector()), new[] {1, 2}},
                {Identity.Create(Guid.NewGuid(), Create.RosterVector()), new[] {1, 2, 3}}
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(invalidEntities.Select(x => new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = x.Key,
                    EntityType = entityType,
                    IsEnabled = true,
                    InvalidValidations = x.Value
                }).ToArray(), new InterviewEntity[0]);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(3));
            Assert.That(invalidEntities.Keys, Is.EquivalentTo(interviewEntities.Select(x=>x.Identity)));
            Assert.That(invalidEntities.Values, Is.EquivalentTo(interviewEntities.Select(x => x.InvalidValidations)));
        }

        [Test]
        public void when_add_rosters()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var addedRosterIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,5))
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var questionnaireStorage =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                    Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.NumericRoster(addedRosterIdentities[0].Id),
                        Create.Entity.NumericRoster(addedRosterIdentities[1].Id)));

            var factory = CreateInterviewFactory(interviewSummaryRepository, questionnaireStorage);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(addedRosterIdentities.Select(x => new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = x,
                    EntityType = EntityType.Section,
                    IsEnabled = true,
                }).ToArray(), new InterviewEntity[0]);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(addedRosterIdentities, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
        }

        [Test]
        public void when_update_answer()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questions = new[]
            {
                new InterviewEntity{AnswerType = AnswerType.Int, AsInt = 1},
                new InterviewEntity{AnswerType = AnswerType.String, AsString = "string"},
                new InterviewEntity{AnswerType = AnswerType.Double, AsDouble = 111.11},
                new InterviewEntity{AnswerType = AnswerType.Long, AsLong = 2222L},
                new InterviewEntity{AnswerType = AnswerType.IntMatrix, AsIntMatrix = new[] { new[]{1,2,3}, new[]{1,2}} },
                new InterviewEntity{AnswerType = AnswerType.TextList, AsList = new []{ new InterviewTextListAnswer(1, "list 1") }},
                new InterviewEntity{AnswerType = AnswerType.YesNoList, AsYesNo = new []{ new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new InterviewEntity{AnswerType = AnswerType.Gps, AsGps = new GeoPosition{ Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now }},
                new InterviewEntity{AnswerType = AnswerType.Audio, AsAudio = AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new InterviewEntity{AnswerType = AnswerType.Area, AsArea = new Area("geometry", "map", 1, 1, "1:1", 1)},
                new InterviewEntity{AnswerType = AnswerType.Datetime, AsDateTime = new DateTime(2012,12,12)},
                new InterviewEntity{AnswerType = AnswerType.IntArray, AsIntArray = new[]{1,2,3}},
                new InterviewEntity{AnswerType = AnswerType.Bool, AsBool = true}
            };
            foreach (var question in questions)
            {
                question.InterviewId = interviewId;
                question.EntityType = EntityType.Question;
                question.Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector());
            }

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(questions, new InterviewEntity[0]);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(13));

            foreach (var expectedQuestion in questions)
            {
                var actualQuestion = interviewEntities.Find(x =>
                    x.InterviewId == expectedQuestion.InterviewId && x.Identity == expectedQuestion.Identity);

                Assert.That(actualQuestion.AnswerType, Is.EqualTo(expectedQuestion.AnswerType));
                Assert.That(actualQuestion.AsArea, Is.EqualTo(expectedQuestion.AsArea));
                Assert.That(actualQuestion.AsString, Is.EqualTo(expectedQuestion.AsString));
                Assert.That(actualQuestion.AsBool, Is.EqualTo(expectedQuestion.AsBool));
                Assert.That(actualQuestion.AsIntArray, Is.EqualTo(expectedQuestion.AsIntArray));
                Assert.That(actualQuestion.AsDateTime, Is.EqualTo(expectedQuestion.AsDateTime));
                Assert.That(actualQuestion.AsDouble, Is.EqualTo(expectedQuestion.AsDouble));
                Assert.That(actualQuestion.AsGps, Is.EqualTo(expectedQuestion.AsGps));
                Assert.That(actualQuestion.AsInt, Is.EqualTo(expectedQuestion.AsInt));
                Assert.That(actualQuestion.AsIntMatrix, Is.EqualTo(expectedQuestion.AsIntMatrix));
                Assert.That(actualQuestion.AsList, Is.EqualTo(expectedQuestion.AsList));
                Assert.That(actualQuestion.AsLong, Is.EqualTo(expectedQuestion.AsLong));
                Assert.That(actualQuestion.AsYesNo, Is.EqualTo(expectedQuestion.AsYesNo));
                Assert.That(actualQuestion.AsAudio, Is.EqualTo(expectedQuestion.AsAudio));
            }
        }

        [Test]
        public void when_remove_answers()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questions = new[]
            {
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsInt = 1},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsString = "string"},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsDouble = 111.11},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsLong = 2222L},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsIntMatrix = new[] { new[]{1,2,3}, new[]{1,2}} },
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsList = new []{ new InterviewTextListAnswer(1, "list 1") }},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsYesNo = new []{ new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsGps = new GeoPosition{ Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now }},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsAudio = AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsArea = new Area("geometry", "map", 1, 1, "1:1", 1)},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsDateTime = new DateTime(2012,12,12)},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsIntArray = new[]{1,2,3}},
                new InterviewEntity{InterviewId = interviewId, Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector()), AsBool = true}
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);
            
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(questions, new InterviewEntity[0]);
            });

            questions[0].AsInt = null;
            questions[1].AsString = null;
            questions[2].AsDouble = null;
            questions[3].AsLong = null;
            questions[4].AsIntMatrix = null;
            questions[5].AsList = null;
            questions[6].AsYesNo = null;
            questions[7].AsGps = null;
            questions[8].AsAudio = null;
            questions[9].AsArea = null;
            questions[10].AsDateTime = null;
            questions[11].AsIntArray = null;
            questions[12].AsBool = null;

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.Save(questions, new InterviewEntity[0]));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(13));
            Assert.That(interviewEntities[0].AsInt, Is.Null);
            Assert.That(interviewEntities[1].AsString, Is.Null);
            Assert.That(interviewEntities[2].AsDouble, Is.Null);
            Assert.That(interviewEntities[3].AsLong, Is.Null);
            Assert.That(interviewEntities[4].AsIntMatrix, Is.Null);
            Assert.That(interviewEntities[5].AsList, Is.Null);
            Assert.That(interviewEntities[6].AsYesNo, Is.Null);
            Assert.That(interviewEntities[7].AsGps, Is.Null);
            Assert.That(interviewEntities[8].AsAudio, Is.Null);
            Assert.That(interviewEntities[9].AsArea, Is.Null);
            Assert.That(interviewEntities[10].AsDateTime, Is.Null);
            Assert.That(interviewEntities[11].AsIntArray, Is.Null);
            Assert.That(interviewEntities[12].AsBool, Is.Null);
        }

        [Test]
        public void when_getting_all_enabled_multimedia_answers_by_questionnaire()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 55);
            var expectedMultimediaAnswers = new[]
            {
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 1", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 2", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 3", InterviewId = Guid.NewGuid()},
                    Enabled = false,
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777)
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 4", InterviewId = Guid.NewGuid()},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var expectedMultimediaAnswer in expectedMultimediaAnswers.GroupBy(x => x.Answer.InterviewId))
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = expectedMultimediaAnswer.Key,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = expectedMultimediaAnswer.FirstOrDefault().QuestionnaireId.ToString()

                    }, expectedMultimediaAnswer.Key);
                }

                factory.Save(expectedMultimediaAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = x.Answer.InterviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = x.Enabled,
                    AsString = x.Answer.Answer
                }).ToArray(), new InterviewEntity[0]);
            });

            //act
            var allMultimediaAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetMultimediaAnswersByQuestionnaire(questionnaireId, expectedMultimediaAnswers.Select(x => x.QuestionId.Id).ToArray()));

            //assert
            Assert.That(allMultimediaAnswers.Length, Is.EqualTo(3));
            Assert.That(allMultimediaAnswers, Is.EquivalentTo(expectedMultimediaAnswers.Where(x=>x.QuestionnaireId == questionnaireId && x.Enabled).Select(x=>x.Answer)));
        }

        [Test]
        public void when_getting_all_enabled_audio_answers_by_questionnaire()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 55);

            var expectedAudioAnswers = new[]
            {
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 1", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 2", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 3", InterviewId = Guid.NewGuid()},
                    Enabled = false,
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777)
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 4", InterviewId = Guid.NewGuid()},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var expectedAudioAnswer in expectedAudioAnswers.GroupBy(x => x.Answer.InterviewId))
                {
                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = expectedAudioAnswer.Key,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = expectedAudioAnswer.FirstOrDefault().QuestionnaireId.ToString()

                    }, expectedAudioAnswer.Key);
                }

                factory.Save(expectedAudioAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = x.Answer.InterviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = x.Enabled,
                    AsAudio = AudioAnswer.FromString(x.Answer.Answer, TimeSpan.FromDays(3))
                }).ToArray(), new InterviewEntity[0]);
            });

            //act
            var allAudioAnswers = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetAudioAnswersByQuestionnaire(questionnaireId));

            //assert
            Assert.That(allAudioAnswers.Length, Is.EqualTo(3));
            Assert.That(allAudioAnswers,
                Is.EquivalentTo(expectedAudioAnswers.Where(x => x.QuestionnaireId == questionnaireId && x.Enabled)
                    .Select(x => x.Answer)));
        }

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
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = interviewId,
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
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

            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(expectedGpsAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = interviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = true,
                    AsGps = x.Answer
                }).ToArray(), new InterviewEntity[0]);
            });

            //act
            var allGpsQuestionIds = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetAnsweredGpsQuestionIdsByQuestionnaire(questionnaireId));

            //assert
            Assert.That(allGpsQuestionIds.Length, Is.EqualTo(3));
            Assert.That(allGpsQuestionIds, Is.EquivalentTo(expectedGpsAnswers.Select(x => x.QuestionId.Id)));
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
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new GeoPosition{Longitude = 1, Latitude = 1, Accuracy = 1, Altitude = 1, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777),
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
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

            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                this.plainTransactionManager.GetSession().Connection.Execute($"DELETE FROM {InterviewsTableName}");

                factory.Save(expectedGpsAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = x.InterviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = true,
                    AsGps = x.Answer
                }).ToArray(), new InterviewEntity[0]);
            });

            //act
            var questionnaireIdentities = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetQuestionnairesWithAnsweredGpsQuestions());

            //assert
            Assert.That(questionnaireIdentities.Length, Is.EqualTo(2));
            Assert.That(questionnaireIdentities, Is.EquivalentTo(expectedGpsAnswers.Select(x => x.QuestionnaireId.ToString()).Distinct()));
        }

        [Test]
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

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
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
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

            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(allGpsAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = x.InterviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = true,
                    AsGps = x.Answer
                }).ToArray(), new InterviewEntity[0]);
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
        public void when_getting_gps_answers_by_questionnaire_id_and_question_id_and_limit_by_west_latitude()
        {
            //arrange
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 555);
            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

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
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
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

            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(allGpsAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = x.InterviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = true,
                    AsGps = x.Answer
                }).ToArray(), new InterviewEntity[0]);
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
            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));

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
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
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

            var factory = CreateInterviewFactory(interviewSummaryRepository);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(allGpsAnswers.Select(x => new InterviewEntity
                {
                    InterviewId = x.InterviewId,
                    Identity = x.QuestionId,
                    EntityType = EntityType.Question,
                    IsEnabled = true,
                    AsGps = x.Answer
                }).ToArray(), new InterviewEntity[0]);
            });

            //act
            var gpsAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetGpsAnswersByQuestionIdAndQuestionnaire(questionnaireId, questionId.Id, 1, 90, -90, 180, -180));

            //assert
            Assert.That(gpsAnswers.Length, Is.EqualTo(1));
        }

        [Test]
        public void when_remove_rosters()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777);
            var rosterVector = Create.RosterVector(1, 2, 3);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRoster(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(),
                    Create.Entity.StaticText(),
                    Create.Entity.Variable(),
                    Create.Entity.Group()

                }), Create.Entity.ListRoster(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(),
                    Create.Entity.StaticText(),
                    Create.Entity.Variable(),
                    Create.Entity.Group()

                }));

            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository, questionnaireStorage);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var entities = questionnaire.Children[0].Children.SelectMany(x => x.Children).Select(x =>
                    new
                    {
                        InterviewId = interviewId,
                        Identity = Identity.Create(x.PublicKey, rosterVector),
                        EntityType = x is Group
                            ? EntityType.Section
                            : (x is StaticText
                                ? EntityType.StaticText
                                : (x is Variable ? EntityType.Variable : EntityType.Question))
                    });

                factory.Save(entities.Select(x => new InterviewEntity
                {
                    InterviewId = x.InterviewId,
                    Identity = x.Identity,
                    EntityType = x.EntityType,
                    IsEnabled = true,
                }).ToArray(), new InterviewEntity[0]);
            });

            var removedEntities = questionnaire.Children[0].Children.Select(x => new InterviewEntity
            {
                InterviewId = interviewId,
                Identity = Identity.Create(x.PublicKey, rosterVector)
            }).ToArray();

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(new InterviewEntity[0], removedEntities);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Where(x => removedEntities.Contains(x)), Is.Empty);
        }

        private InterviewFactory CreateInterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository = null,
            IQuestionnaireStorage questionnaireStorage = null)
            => new InterviewFactory(
                summaryRepository: interviewSummaryRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                sessionProvider: this.plainTransactionManager);

        private static IQueryableReadSideRepositoryReader<InterviewSummary> GetInMemoryInterviewSummaryRepository(Guid interviewId)
        {
            var interviewSummaryRepository = Create.Storage.InMemoryReadeSideStorage<InterviewSummary>();
            interviewSummaryRepository.Store(new InterviewSummary
            {
                SummaryId = interviewId.FormatGuid(),
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ReceivedByInterviewer = false,
                QuestionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 555).ToString()

            }, interviewId.FormatGuid());
            return interviewSummaryRepository;
        }

        private PostgreReadSideStorage<InterviewSummary> GetPostgresInterviewSummaryRepository() 
            => new PostgreReadSideStorage<InterviewSummary>(this.plainTransactionManager, Mock.Of<ILogger>(), "summaryid");

        private InterviewEntity[] GetInterviewEntities(InterviewFactory factory, Guid interviewId) => 
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetInterviewEntities(interviewId).ToArray());
    }
}