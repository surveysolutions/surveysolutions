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
                    typeof(InterviewCommentedStatusMap),
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

            var sectionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1));
            var questionId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var staticTextId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3));
            var variableId = InterviewStateIdentity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3, 4));


            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.Group(sectionId.Id),
                Create.Entity.StaticText(staticTextId.Id),
                Create.Entity.Variable(variableId.Id),
                Create.Entity.TextQuestion(questionId.Id)
            });

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter());
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));


            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Enablement = new Dictionary<InterviewStateIdentity, bool>()
            {
                {sectionId, true},
                {questionId, true},
                {staticTextId, false},
                {variableId, false}
            };

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
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

            var readOnlyQuestions = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(readOnlyQuestions
                .Select(x => Create.Entity.TextQuestion(x.Id)).OfType<IComposite>().ToArray());

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.ReadOnly = readOnlyQuestions.Select(InterviewStateIdentity.Create).ToList();

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(readOnlyQuestions, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
        }

        [Test]
        public void when_enable_entities()
        {
            //arrange
            var interviewId = Guid.NewGuid();

            var sectionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1));
            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var staticTextId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3));
            var variableId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3, 4));


            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.Group(sectionId.Id),
                Create.Entity.StaticText(staticTextId.Id),
                Create.Entity.Variable(variableId.Id),
                Create.Entity.TextQuestion(questionId.Id)
            });

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter());
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));


            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Enablement = new Dictionary<InterviewStateIdentity, bool>()
            {
                {InterviewStateIdentity.Create(sectionId), true},
                {InterviewStateIdentity.Create(questionId), true},
                {InterviewStateIdentity.Create(staticTextId), true},
                {InterviewStateIdentity.Create(variableId), true}
            };

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(4));
            Assert.That(new[] {sectionId, questionId, staticTextId, variableId}, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
        }

       [Test]
        public void when_make_entities_valid()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            
            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var staticTextId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3));
            var variableId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3, 4));


            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.StaticText(staticTextId.Id),
                Create.Entity.Variable(variableId.Id),
                Create.Entity.TextQuestion(questionId.Id)
            });

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter());
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));


            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Validity = new Dictionary<InterviewStateIdentity, InterviewStateValidation>
            {
                {
                    InterviewStateIdentity.Create(questionId),
                    new InterviewStateValidation {Id = questionId.Id, RosterVector = questionId.RosterVector}
                },
                {
                    InterviewStateIdentity.Create(staticTextId),
                    new InterviewStateValidation {Id = staticTextId.Id, RosterVector = staticTextId.RosterVector}
                },
                {
                    InterviewStateIdentity.Create(variableId),
                    new InterviewStateValidation {Id = variableId.Id, RosterVector = variableId.RosterVector}
                }
            };

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(3));
            Assert.That(new[] {questionId, staticTextId, variableId}, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
        }
        
        [Test]
        public void when_make_entities_invalid()
        {
            //arrange
            var interviewId = Guid.NewGuid();

            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var staticTextId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3));


            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.StaticText(staticTextId.Id),
                Create.Entity.TextQuestion(questionId.Id)
            });

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var questionnaireStorage = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter());
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));


            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Validity = new Dictionary<InterviewStateIdentity, InterviewStateValidation>
            {
                {
                    InterviewStateIdentity.Create(questionId),
                    new InterviewStateValidation
                    {
                        Id = questionId.Id,
                        RosterVector = questionId.RosterVector,
                        Validations = new[] {1, 2, 3}
                    }
                },
                {
                    InterviewStateIdentity.Create(staticTextId),
                    new InterviewStateValidation
                    {
                        Id = staticTextId.Id,
                        RosterVector = staticTextId.RosterVector,
                        Validations = new[] {1}
                    }
                }
            };

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(new[] {questionId, staticTextId}, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
            Assert.That(interviewState.Validity.Values.Select(x => x.Validations), Is.EquivalentTo(interviewEntities.Select(x => x.InvalidValidations)));
        }

        [Test]
        public void when_make_entities_with_warnings()
        {
            //arrange
            var interviewId = Guid.NewGuid();

            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var staticTextId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3));


            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.StaticText(staticTextId.Id),
                Create.Entity.TextQuestion(questionId.Id)
            });

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));


            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Warnings = new Dictionary<InterviewStateIdentity, InterviewStateValidation>
            {
                {
                    InterviewStateIdentity.Create(questionId),
                    new InterviewStateValidation
                    {
                        Id = questionId.Id,
                        RosterVector = questionId.RosterVector,
                        Validations = new[] {1, 2, 3}
                    }
                },
                {
                    InterviewStateIdentity.Create(staticTextId),
                    new InterviewStateValidation
                    {
                        Id = staticTextId.Id,
                        RosterVector = staticTextId.RosterVector,
                        Validations = new[] {1}
                    }
                }
            };

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(new[] {questionId, staticTextId}, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
            Assert.That(interviewState.Warnings.Values.Select(x => x.Validations), Is.EquivalentTo(interviewEntities.Select(x => x.WarningValidations)));
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
                var interviewState = Create.Entity.InterviewState(interviewId);
                interviewState.Enablement = addedRosterIdentities.ToDictionary(InterviewStateIdentity.Create, x => true);

                factory.Save(interviewState);
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
                new InterviewEntity{EntityType = EntityType.Question, AsInt = 1},
                new InterviewEntity{EntityType = EntityType.Question, AsString = "string"},
                new InterviewEntity{EntityType = EntityType.Question, AsDouble = 111.11},
                new InterviewEntity{EntityType = EntityType.Question, AsIntMatrix = new[] { new[]{1,2,3}, new[]{1,2}} },
                new InterviewEntity{EntityType = EntityType.Question, AsList = new []{ new InterviewTextListAnswer(1, "list 1") }},
                new InterviewEntity{EntityType = EntityType.Question, AsYesNo = new []{ new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new InterviewEntity{EntityType = EntityType.Question, AsGps = new GeoPosition{ Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now }},
                new InterviewEntity{EntityType = EntityType.Question, AsAudio = AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new InterviewEntity{EntityType = EntityType.Question, AsArea = new Area("geometry", "map", 1, 1, "1:1", 1)},
                new InterviewEntity{EntityType = EntityType.Question, AsDateTime = new DateTime(2012,12,12)},
                new InterviewEntity{EntityType = EntityType.Question, AsIntArray = new[]{1,2,3}},
                new InterviewEntity{EntityType = EntityType.Variable, AsLong = 2222L},
                new InterviewEntity{EntityType = EntityType.Variable, AsBool = true},
                new InterviewEntity{EntityType = EntityType.Variable, AsString = "string variable"},
                new InterviewEntity{EntityType = EntityType.Variable, AsDateTime = new DateTime(2017,12,12)},
                new InterviewEntity{EntityType = EntityType.Variable, AsDouble = 222.22}
            };
            foreach (var question in questions)
            {
                question.InterviewId = interviewId;
                question.Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector());
            }

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questions.Select(ToQuestionnaireEntity).ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Answers = questions.ToDictionary(x => InterviewStateIdentity.Create(x.Identity),
                x => new InterviewStateAnswer
                {
                    Id = x.Identity.Id,
                    RosterVector = x.Identity.RosterVector,
                    AsString = x.AsString,
                    AsDouble = x.AsDouble,
                    AsBool = x.AsBool,
                    AsLong = x.AsLong,
                    AsInt = x.AsInt,
                    AsDatetime = x.AsDateTime,
                    AsIntArray = x.AsIntArray,
                    AsAudio = x.AsAudio == null ? null : entitySerializer.Serialize(x.AsAudio),
                    AsGps = x.AsGps == null ? null : entitySerializer.Serialize(x.AsGps),
                    AsArea = x.AsArea == null ? null : entitySerializer.Serialize(x.AsArea),
                    AsList = x.AsList == null ? null : entitySerializer.Serialize(x.AsList),
                    AsIntMatrix = x.AsIntMatrix == null ? null : entitySerializer.Serialize(x.AsIntMatrix),
                    AsYesNo = x.AsYesNo == null ? null : entitySerializer.Serialize(x.AsYesNo)
                });

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(16));

            foreach (var expectedQuestion in questions)
            {
                var actualQuestion = interviewEntities.Find(x =>
                    x.InterviewId == expectedQuestion.InterviewId && x.Identity == expectedQuestion.Identity);

                Assert.That(actualQuestion.EntityType, Is.EqualTo(expectedQuestion.EntityType));
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
                new InterviewEntity{EntityType = EntityType.Question, AsInt = 1},
                new InterviewEntity{EntityType = EntityType.Question, AsString = "string"},
                new InterviewEntity{EntityType = EntityType.Question, AsDouble = 111.11},
                new InterviewEntity{EntityType = EntityType.Question, AsIntMatrix = new[] { new[]{1,2,3}, new[]{1,2}} },
                new InterviewEntity{EntityType = EntityType.Question, AsList = new []{ new InterviewTextListAnswer(1, "list 1") }},
                new InterviewEntity{EntityType = EntityType.Question, AsYesNo = new []{ new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new InterviewEntity{EntityType = EntityType.Question, AsGps = new GeoPosition{ Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now }},
                new InterviewEntity{EntityType = EntityType.Question, AsAudio = AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new InterviewEntity{EntityType = EntityType.Question, AsArea = new Area("geometry", "map", 1, 1, "1:1", 1)},
                new InterviewEntity{EntityType = EntityType.Question, AsDateTime = new DateTime(2012,12,12)},
                new InterviewEntity{EntityType = EntityType.Question, AsIntArray = new[]{1,2,3}},
                new InterviewEntity{EntityType = EntityType.Variable, AsLong = 2222L},
                new InterviewEntity{EntityType = EntityType.Variable, AsBool = true},
                new InterviewEntity{EntityType = EntityType.Variable, AsString = "string variable"},
                new InterviewEntity{EntityType = EntityType.Variable, AsDateTime = new DateTime(2017,12,12)},
                new InterviewEntity{EntityType = EntityType.Variable, AsDouble = 222.22}
            };
            foreach (var question in questions)
            {
                question.InterviewId = interviewId;
                question.Identity = Identity.Create(Guid.NewGuid(), Create.RosterVector());
            }

            var interviewSummaryRepository = GetInMemoryInterviewSummaryRepository(interviewId);
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questions.Select(ToQuestionnaireEntity).ToArray());

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Answers = questions.ToDictionary(x => InterviewStateIdentity.Create(x.Identity),
                x => new InterviewStateAnswer
                {
                    Id = x.Identity.Id,
                    RosterVector = x.Identity.RosterVector,
                    AsString = x.AsString,
                    AsDouble = x.AsDouble,
                    AsBool = x.AsBool,
                    AsLong = x.AsLong,
                    AsInt = x.AsInt,
                    AsDatetime = x.AsDateTime,
                    AsIntArray = x.AsIntArray,
                    AsAudio = entitySerializer.Serialize(x.AsAudio),
                    AsGps = entitySerializer.Serialize(x.AsGps),
                    AsArea = entitySerializer.Serialize(x.AsArea),
                    AsList = entitySerializer.Serialize(x.AsList),
                    AsIntMatrix = entitySerializer.Serialize(x.AsIntMatrix),
                    AsYesNo = entitySerializer.Serialize(x.AsYesNo)
                });

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Answers = questions.ToDictionary(x => InterviewStateIdentity.Create(x.Identity),
                x => new InterviewStateAnswer {Id = x.Identity.Id, RosterVector = x.Identity.RosterVector});
            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Length, Is.EqualTo(16));
            foreach (var interviewEntity in interviewEntities)
            {
                Assert.That(interviewEntity.AsInt, Is.Null);
                Assert.That(interviewEntity.AsString, Is.Null);
                Assert.That(interviewEntity.AsDouble, Is.Null);
                Assert.That(interviewEntity.AsIntMatrix, Is.Null);
                Assert.That(interviewEntity.AsList, Is.Null);
                Assert.That(interviewEntity.AsYesNo, Is.Null);
                Assert.That(interviewEntity.AsGps, Is.Null);
                Assert.That(interviewEntity.AsAudio, Is.Null);
                Assert.That(interviewEntity.AsArea, Is.Null);
                Assert.That(interviewEntity.AsDateTime, Is.Null);
                Assert.That(interviewEntity.AsIntArray, Is.Null);
                Assert.That(interviewEntity.AsLong, Is.Null);
                Assert.That(interviewEntity.AsBool, Is.Null);
            }
        }

        [Test]
        public void when_getting_all_enabled_multimedia_answers_by_questionnaire()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = Create.Entity.QuestionnaireIdentity(Guid.NewGuid(), 222);
            var multimediaQuestionId = Guid.NewGuid();
            
            var expectedMultimediaAnswers = new[]
            {
                new
                {
                    QuestionId = InterviewStateIdentity.Create(multimediaQuestionId, Create.RosterVector()),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 1", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = InterviewStateIdentity.Create(multimediaQuestionId, Create.RosterVector(1)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 2", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = InterviewStateIdentity.Create(multimediaQuestionId, Create.RosterVector(1,2)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 3", InterviewId = Guid.NewGuid()},
                    Enabled = false,
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777)
                },
                new
                {
                    QuestionId = InterviewStateIdentity.Create(multimediaQuestionId, Create.RosterVector(1,2,3)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 4", InterviewId = Guid.NewGuid()},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.MultimediaQuestion(multimediaQuestionId));

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

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

                foreach (var groupedInterviews in expectedMultimediaAnswers.GroupBy(x=>x.Answer.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId,
                        x => new InterviewStateAnswer
                        {
                            Id = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector,
                            AsString = x.Answer.Answer
                        });
                    interviewState.Enablement = groupedInterviews.ToDictionary(x => x.QuestionId, x => x.Enabled);

                    factory.Save(interviewState);
                }
            });

            //act
            var allMultimediaAnswers = this.plainTransactionManager.ExecuteInQueryTransaction(
                () => factory.GetMultimediaAnswersByQuestionnaire(questionnaireId, expectedMultimediaAnswers.Select(x => x.QuestionId.Id).ToArray()));

            //assert
            Assert.That(allMultimediaAnswers.Length, Is.EqualTo(3));
            Assert.That(allMultimediaAnswers, Is.EquivalentTo(expectedMultimediaAnswers.Where(x => x.QuestionnaireId == questionnaireId && x.Enabled).Select(x => x.Answer)));
        }

        [Test]
        public void when_getting_all_enabled_audio_answers_by_questionnaire()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 55);
            var audioQuestionId = Guid.NewGuid();

            var expectedAudioAnswers = new[]
            {
                new
                {
                    QuestionId = InterviewStateIdentity.Create(audioQuestionId, Create.RosterVector()),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 1", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = InterviewStateIdentity.Create(audioQuestionId, Create.RosterVector(1)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 2", InterviewId = interviewId},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
                new
                {
                    QuestionId = InterviewStateIdentity.Create(audioQuestionId, Create.RosterVector(1,2)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 3", InterviewId = Guid.NewGuid()},
                    Enabled = false,
                    QuestionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 777)
                },
                new
                {
                    QuestionId = InterviewStateIdentity.Create(audioQuestionId, Create.RosterVector(1,2,3)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 4", InterviewId = Guid.NewGuid()},
                    Enabled = true,
                    QuestionnaireId = questionnaireId
                },
            };

            var interviewSummaryRepository = GetPostgresInterviewSummaryRepository();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.AudioQuestion(audioQuestionId, "myaudio"));

            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository,
                questionnaireStorage: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire));

            var entitySerializer = new EntitySerializer<object>();

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

                foreach (var groupedInterviews in expectedAudioAnswers.GroupBy(x => x.Answer.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId,
                        x => new InterviewStateAnswer
                        {
                            Id = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector,
                            AsAudio = entitySerializer.Serialize(AudioAnswer.FromString(x.Answer.Answer,
                                TimeSpan.FromMinutes(2)))
                        });
                    interviewState.Enablement = groupedInterviews.ToDictionary(x => x.QuestionId, x => x.Enabled);

                    factory.Save(interviewState);
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

        [Test]
        public void when_remove_rosters()
        {
            //arrange
            var interviewId = Guid.NewGuid();
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
                        Identity = InterviewStateIdentity.Create(x.PublicKey, rosterVector),
                        EntityType = x is Group
                            ? EntityType.Section
                            : (x is StaticText
                                ? EntityType.StaticText
                                : (x is Variable ? EntityType.Variable : EntityType.Question))
                    });

                var interviewState = Create.Entity.InterviewState(interviewId);
                interviewState.Enablement = entities.ToDictionary(x => x.Identity, x => true);

                factory.Save(interviewState);
            });

            var removedEntities = questionnaire.Children[0].Children.Select(x => new InterviewStateIdentity
            {
                Id = x.PublicKey,
                RosterVector = rosterVector
            }).ToArray();

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var interviewState = Create.Entity.InterviewState(interviewId);
                interviewState.Removed = removedEntities.ToList();

                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId);

            Assert.That(interviewEntities.Select(x => InterviewStateIdentity.Create(x.Identity))
                .Where(x => removedEntities.Contains(x)), Is.Empty);
        }

        private IComposite ToQuestionnaireEntity(InterviewEntity entity)
        {
            if (entity.EntityType == EntityType.Question)
                return Create.Entity.TextQuestion(entity.Identity.Id);
            if (entity.EntityType == EntityType.Variable)
                return Create.Entity.Variable(entity.Identity.Id);

            return null;
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
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                factory.GetInterviewEntities(new QuestionnaireIdentity(Guid.NewGuid(), 1), interviewId).ToArray());
    }
}