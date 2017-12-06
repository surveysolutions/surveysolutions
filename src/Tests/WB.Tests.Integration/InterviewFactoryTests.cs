using System;
using System.Collections.Generic;
using System.Linq;
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
using WB.UI.Headquarters.Migrations.ReadSide;

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
                factory.EnableEntities(interviewId, new[] {interviewEntityIds[0]}, EntityType.Question, true);
                factory.EnableEntities(interviewId, new[] {interviewEntityIds[1]}, EntityType.Section, true);
                factory.EnableEntities(interviewId, new[] {interviewEntityIds[2]}, EntityType.StaticText, true);
                factory.EnableEntities(interviewId, new[] {interviewEntityIds[3]}, EntityType.Variable, true);
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
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.MarkQuestionsAsReadOnly(interviewId, readOnlyQuestions));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);
            
            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(readOnlyQuestions, Is.EquivalentTo(interviewEntities.Select(x=>x.Identity)));
        }

        [Test]
        public void when_udpading_variable_with_null_value()
        {
            Guid interviewId = Id.g1;
            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            // Act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.UpdateVariables(interviewId, new[]
            {
                new ChangedVariable(Create.Identity(), null)
            }));

            // Assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);
            
            Assert.That(GetAnswer(interviewEntities[0]), Is.Null);
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
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.EnableEntities(interviewId, enabledEntities, entityType, true));

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
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.MakeEntitiesValid(interviewId, validEntities, entityType));

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
            var invalidEntities = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {Identity.Create(Guid.NewGuid(), Create.RosterVector()), new[] {new FailedValidationCondition(1)}},
                {Identity.Create(Guid.NewGuid(), Create.RosterVector()), new[] {new FailedValidationCondition(1), new FailedValidationCondition(2), }},
                {Identity.Create(Guid.NewGuid(), Create.RosterVector()), new[] {new FailedValidationCondition(1), new FailedValidationCondition(2), new FailedValidationCondition(3), }}
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.MakeEntitiesInvalid(interviewId, invalidEntities, entityType));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(3));
            Assert.That(invalidEntities.Keys, Is.EquivalentTo(interviewEntities.Select(x=>x.Identity)));
            Assert.That(invalidEntities.Values.Select(x=>x.Select(y=>y.FailedConditionIndex).ToArray()), Is.EquivalentTo(interviewEntities.Select(x => x.InvalidValidations)));
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
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.AddRosters(interviewId, addedRosterIdentities));

            //assert
            var interview = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetInterviewData(interviewId));

            Assert.That(interview.Levels.Count, Is.EqualTo(2));
        }
        
        [Test]
        public void when_update_variables()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var changedVariables = new[]
            {
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector()), "string variable"),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector()), 222.333m),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector()), 111111L),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector()), new DateTime(1983, 06, 19, 3, 15, 0)),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector()), true),
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.UpdateVariables(interviewId, changedVariables));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(5));
            foreach (var changedVariable in changedVariables)
            {
                var upsertedEntity = interviewEntities.FirstOrDefault(x => x.Identity == changedVariable.Identity);

                Assert.That(upsertedEntity, Is.Not.Null);
                Assert.AreEqual(changedVariable.NewValue, GetAnswer(upsertedEntity));
            }
        }

        [Test]
        public void when_update_answer()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questions = new[]
            {
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)1},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)"string"},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)111.11m},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)2222L},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new[] { new[]{1,2,3}, new[]{1,2}} },
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new []{ new InterviewTextListAnswer(1, "list 1") }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new []{ new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new GeoPosition{ Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new Area("geometry", "map", 1, 1, "1:1", 1)},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new DateTime(2012,12,12)},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new[]{1,2,3}},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)true}
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var question in questions)
                    factory.UpdateAnswer(interviewId, question.Id, question.Answer);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(13));
            foreach (var question in questions)
            {
                var upsertedEntity = interviewEntities.FirstOrDefault(x => x.Identity == question.Id);

                Assert.That(upsertedEntity, Is.Not.Null);
                Assert.AreEqual(question.Answer, GetAnswer(upsertedEntity));
            }
        }

        [Test]
        public void when_remove_answers()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questions = new[]
            {
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)1},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)"string"},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)111.11m},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)2222L},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new[] { new[]{1,2,3}, new[]{1,2}} },
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new []{ new InterviewTextListAnswer(1, "list 1") }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new []{ new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new GeoPosition{ Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new Area("geometry", "map", 1, 1, "1:1", 1)},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new DateTime(2012,12,12)},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)new[]{1,2,3}},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector()), Answer = (object)true}
            };

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository);
            
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var question in questions)
                    factory.UpdateAnswer(interviewId, question.Id, question.Answer);
            });

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.RemoveAnswers(interviewId, questions.Select(x=>x.Id).ToArray()));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(13));
            foreach (var question in questions)
            {
                var upsertedEntity = interviewEntities.FirstOrDefault(x => x.Identity == question.Id);

                Assert.That(upsertedEntity, Is.Not.Null);
                Assert.That(GetAnswer(upsertedEntity), Is.Null);
            }
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

                foreach (var expectedMultimediaAnswer in expectedMultimediaAnswers)
                {
                    factory.UpdateAnswer(expectedMultimediaAnswer.Answer.InterviewId, expectedMultimediaAnswer.QuestionId,
                        expectedMultimediaAnswer.Answer.Answer);

                    factory.EnableEntities(expectedMultimediaAnswer.Answer.InterviewId, new[] {expectedMultimediaAnswer.QuestionId},
                        EntityType.Question, expectedMultimediaAnswer.Enabled);
                }

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

                foreach (var expectedMultimediaAnswer in expectedAudioAnswers)
                {
                    factory.UpdateAnswer(expectedMultimediaAnswer.Answer.InterviewId, expectedMultimediaAnswer.QuestionId,
                        AudioAnswer.FromString(expectedMultimediaAnswer.Answer.Answer, TimeSpan.FromDays(3)));

                    factory.EnableEntities(expectedMultimediaAnswer.Answer.InterviewId, new[] { expectedMultimediaAnswer.QuestionId },
                        EntityType.Question, expectedMultimediaAnswer.Enabled);
                }

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
                foreach (var expectedGpsAnswer in expectedGpsAnswers)
                {
                    factory.UpdateAnswer(expectedGpsAnswer.InterviewId, expectedGpsAnswer.QuestionId, expectedGpsAnswer.Answer);
                    factory.EnableEntities(expectedGpsAnswer.InterviewId, new[] { expectedGpsAnswer.QuestionId },
                        EntityType.Question, true);
                }

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

                foreach (var expectedGpsAnswer in expectedGpsAnswers)
                {
                    factory.UpdateAnswer(expectedGpsAnswer.InterviewId, expectedGpsAnswer.QuestionId, expectedGpsAnswer.Answer);
                    factory.EnableEntities(expectedGpsAnswer.InterviewId, new[] { expectedGpsAnswer.QuestionId },
                        EntityType.Question, true);
                }

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
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    factory.UpdateAnswer(gpsAnswer.InterviewId, gpsAnswer.QuestionId, gpsAnswer.Answer);
                    factory.EnableEntities(gpsAnswer.InterviewId, new[] { gpsAnswer.QuestionId },
                        EntityType.Question, true);
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
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    factory.UpdateAnswer(gpsAnswer.InterviewId, gpsAnswer.QuestionId, gpsAnswer.Answer);
                    factory.EnableEntities(gpsAnswer.InterviewId, new[] { gpsAnswer.QuestionId },
                        EntityType.Question, true);
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
                foreach (var gpsAnswer in allGpsAnswers)
                {
                    factory.UpdateAnswer(gpsAnswer.InterviewId, gpsAnswer.QuestionId, gpsAnswer.Answer);
                    factory.EnableEntities(gpsAnswer.InterviewId, new[] { gpsAnswer.QuestionId },
                        EntityType.Question, true);
                }

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
                foreach (var entity in questionnaire.Children[0].Children.SelectMany(x => x.Children).Select(x =>
                    new
                    {
                        InterviewId = interviewId,
                        Identity = Identity.Create(x.PublicKey, rosterVector),
                        EntityType = x is Group
                            ? EntityType.Section
                            : (x is StaticText
                                ? EntityType.StaticText
                                : (x is Variable ? EntityType.Variable : EntityType.Question))
                    }))
                {
                    factory.EnableEntities(entity.InterviewId, new[] {entity.Identity}, entity.EntityType, true);
                }
            });

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.RemoveRosters(questionnaireId, interviewId,
                questionnaire.Children[0].Children.Select(x => Identity.Create(x.PublicKey, rosterVector)).ToArray()));

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities, Is.Empty);
        }


        private InterviewFactory CreateInterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository = null,
            IQuestionnaireStorage questionnaireStorage = null)
            => new InterviewFactory(
                summaryRepository: interviewSummaryRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                sessionProvider: this.plainTransactionManager,
                jsonSerializer: new EntitySerializer<object>());

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

        private InterviewEntity[] GetInterviewEntities(InterviewFactory factory, Guid interviewId)
        {
            var interview = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetInterviewData(interviewId));

            return interview.Levels.Values.SelectMany(x =>
                M021_CreateInterviewsTable.ToEntities(interview.InterviewId, x)).ToArray();
        }

        private object GetAnswer(InterviewEntity ie)
            => ie.AsBool ?? ie.AsDateTime ?? ie.AsDouble ?? ie.AsInt ?? ie.AsLong ?? ie.AsString ?? ie.AsIntArray ??
               ie.AsIntMatrix ?? ie.AsIntMatrix ?? ie.AsList ?? ie.AsYesNo ?? ie.AsArea ?? ie.AsGps ?? (object)ie.AsAudio;
    }
}