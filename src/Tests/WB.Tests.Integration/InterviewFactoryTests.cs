using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Npgsql;
using NpgsqlTypes;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
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
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryTests
    {
        private const string InterviewsTableName = "readside.interviews";
        private const string InterviewIdColumn = "interviewId";
        private const string EntityIdColumn = "entityId";
        private const string RosterVectorColumn = "rostervector";
        private const string EntityTypeColumn = "entitytype";
        private const string ReadOnlyColumn = "isreadonly";
        private const string EnabledColumn = "isenabled";
        private const string InvalidValidationsColumn = "invalidvalidations";
        private const string FlagColumn = "hasflag";

        private const string AsIntColumn = "asint";
        private const string AsDoubleColumn = "asdouble";
        private const string AsLongColumn = "aslong";
        private const string AsDateTimeColumn = "asdatetime";
        private const string AsStringColumn = "asstring";
        private const string AsListColumn = "aslist";
        private const string AsIntArrayColumn = "asintarray";
        private const string AsIntMatrixColumn = "asintmatrix";
        private const string AsYesNoColumn = "asyesno";
        private const string AsGpsColumn = "asgps";
        private const string AsBoolColumn = "asbool";
        private const string AsAudioColumn = "asaudio";
        private const string AsAreaColumn = "asarea";

        private PlainPostgresTransactionManager sessionProvider;
        private string connectionString;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            connectionString = DatabaseTestInitializer.InitializeDb(PostgreSQLEventStoreTests.DbType.ReadSide);
            var sessionFactory = IntegrationCreate.SessionFactory(this.connectionString, new List<Type>(), true, "readside");
            this.sessionProvider = new PlainPostgresTransactionManager(sessionFactory);
            SqlMapper.AddTypeHandler(JsonHandler<GeoPosition>.Instance);
            SqlMapper.AddTypeHandler(JsonHandler<AudioAnswer>.Instance);
            SqlMapper.AddTypeHandler(JsonHandler<Area>.Instance);
            SqlMapper.AddTypeHandler(JsonHandler<int[][]>.Instance);
            SqlMapper.AddTypeHandler(JsonHandler<InterviewTextListAnswers>.Instance);
            SqlMapper.AddTypeHandler(JsonHandler<AnsweredYesNoOption[]>.Instance);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.sessionProvider.Dispose();
            DatabaseTestInitializer.DropDb(this.connectionString);
        }

        private InterviewFactory CreateInterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository = null,
            IQuestionnaireStorage questionnaireStorage = null)
            => new InterviewFactory(
                summaryRepository: interviewSummaryRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                sessionProvider: this.sessionProvider,
                jsonSerializer: new EntitySerializer<object>());

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

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {FlagColumn}) " +
                    $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @HasFlag) ",
                    new[]
                    {
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = questionIdentities[0].Id,
                            RosterVector = questionIdentities[0].RosterVector.Array,
                            EntityType = EntityType.Question,
                            HasFlag = true
                        },
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = questionIdentities[1].Id,
                            RosterVector = questionIdentities[1].RosterVector.Array,
                            EntityType = EntityType.Question,
                            HasFlag = true
                        },
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(1).Array,
                            EntityType = EntityType.Question,
                            HasFlag = false
                        }
                    }));


            var factory = CreateInterviewFactory(interviewSummaryRepository: interviewSummaryRepository);

            //act
            var flaggedIdentites = this.sessionProvider.ExecuteInPlainTransaction(() => factory.GetFlaggedQuestionIds(interviewId));

            //assert
            Assert.That(flaggedIdentites.Length, Is.EqualTo(2));
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
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.SetFlagToQuestion(interviewId,questionIdentity, true));
            
            //assert
            var flaggedIdentity = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {FlagColumn} = true",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .FirstOrDefault());

            Assert.That(flaggedIdentity.Id, Is.EqualTo(questionIdentity.Id));
            Assert.That(flaggedIdentity.RosterVector, Is.EqualTo(questionIdentity.RosterVector));
        }

        [Test]
        public void when_remove_flag_from_question()
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
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.SetFlagToQuestion(interviewId, questionIdentity, false));

            //assert
            var flaggedIdentity = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {FlagColumn} = false",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .FirstOrDefault());

            Assert.That(flaggedIdentity.Id, Is.EqualTo(questionIdentity.Id));
            Assert.That(flaggedIdentity.RosterVector, Is.EqualTo(questionIdentity.RosterVector));
        }

        [Test]
        public void when_remove_interview()
        {
            //arrange
            var interviewId = Guid.NewGuid();

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                    $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType) ",
                    new[]
                    {
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(2,3).Array,
                            EntityType = EntityType.Question
                        },
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(1,2,3).Array,
                            EntityType = EntityType.Question
                        },
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(1).Array,
                            EntityType = EntityType.StaticText
                        }
                    }));

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.RemoveInterview(interviewId));

            //assert
            var interviewsCount = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT * FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                    new { InterviewId = interviewId }).Count());

            Assert.That(interviewsCount, Is.EqualTo(0));
        }

        [Test]
        public void when_make_question_readonly()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var readOnlyQuestions = new []
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };
            
            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.MarkQuestionsAsReadOnly(interviewId, readOnlyQuestions));

            //assert
            var upsertedQuestionIdentities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {ReadOnlyColumn} = true",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .ToArray());

            Assert.That(upsertedQuestionIdentities.Length, Is.EqualTo(2));
            Assert.That(readOnlyQuestions, Is.EquivalentTo(upsertedQuestionIdentities));
        }

        [TestCase(EntityType.StaticText)]
        [TestCase(EntityType.Section)]
        [TestCase(EntityType.Question)]
        [TestCase(EntityType.Variable)]
        public void when_enable_entities(EntityType entityType)
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var enabledEntities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.EnableEntities(interviewId, enabledEntities, entityType, true));

            //assert
            var upsertedIdentities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {EnabledColumn} = true",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .ToArray());

            Assert.That(upsertedIdentities.Length, Is.EqualTo(2));
            Assert.That(enabledEntities, Is.EquivalentTo(upsertedIdentities));
        }

        [TestCase(EntityType.StaticText)]
        [TestCase(EntityType.Section)]
        [TestCase(EntityType.Question)]
        [TestCase(EntityType.Variable)]
        public void when_make_entities_valid(EntityType entityType)
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var validEntities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {InvalidValidationsColumn}) " +
                    "VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, '{1,2,3}') ",
                    new[]
                    {
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(2,3).Array,
                            EntityType = entityType
                        },
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(1,2,3).Array,
                            EntityType = entityType
                        },
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = Guid.NewGuid(),
                            RosterVector = Create.RosterVector(1).Array,
                            EntityType = entityType
                        }
                    }));

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.MakeEntitiesValid(interviewId, validEntities, entityType));

            //assert
            var upsertedIdentities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {InvalidValidationsColumn} IS NULL",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .ToArray());

            Assert.That(upsertedIdentities.Length, Is.EqualTo(2));
            Assert.That(validEntities, Is.EquivalentTo(upsertedIdentities));
        }

        [TestCase(EntityType.StaticText)]
        [TestCase(EntityType.Section)]
        [TestCase(EntityType.Question)]
        [TestCase(EntityType.Variable)]
        public void when_make_entities_invalid(EntityType entityType)
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var invalidEntities = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {Identity.Create(Guid.NewGuid(), Create.RosterVector(1)), new[] {new FailedValidationCondition(1)}},
                {Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)), new[] {new FailedValidationCondition(1), new FailedValidationCondition(2), }},
                {Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)), new[] {new FailedValidationCondition(1), new FailedValidationCondition(2), new FailedValidationCondition(3), }}
            };

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.MakeEntitiesInvalid(interviewId, invalidEntities, entityType));
            //assert

            var upsertedEntities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn}, {InvalidValidationsColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {InvalidValidationsColumn} IS NOT NULL",
                    new { InterviewId = interviewId })
                .Select(x =>new {EntityId = Identity.Create((Guid)x.entityid, (int[])x.rostervector), InvalidValidationIndexes = (int[])x.invalidvalidations})
                .ToDictionary(x=>x.EntityId, x=>(IReadOnlyList<FailedValidationCondition>)x.InvalidValidationIndexes.Select(y=>new FailedValidationCondition(y)).ToReadOnlyCollection()));

            Assert.That(upsertedEntities.Keys.Count, Is.EqualTo(3));
            Assert.That(invalidEntities, Is.EquivalentTo(upsertedEntities));
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

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.AddRosters(interviewId, addedRosterIdentities));
            
            //assert
            var upsertedIdentities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId AND {EntityTypeColumn} = 1",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .ToArray());

            Assert.That(upsertedIdentities.Length, Is.EqualTo(2));
            Assert.That(addedRosterIdentities, Is.EquivalentTo(upsertedIdentities));
        }
        
        [Test]
        public void when_update_variables()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var changedVariables = new[]
            {
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector(1)), "string variable"),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)), 222.333m),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)), 111111L),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4)), new DateTime(1983, 06, 19, 3, 15, 0)),
                new ChangedVariable(Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4,5)), true),
            };

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.UpdateVariables(interviewId, changedVariables));

            //assert
            var upsertedEntities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn}, {AsStringColumn}, {AsBoolColumn}, {AsDateTimeColumn}, {AsDoubleColumn}, {AsLongColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                    new { InterviewId = interviewId })
                .Select(x => new ChangedVariable(Identity.Create((Guid)x.entityid, (int[])x.rostervector), x.asstring ?? x.asdatetime ?? x.asdouble ?? x.aslong ?? x.asbool))
                .ToArray());

            Assert.That(upsertedEntities.Length, Is.EqualTo(5));
            foreach (var changedVariable in changedVariables)
            {
                var upsertedEntity = upsertedEntities.FirstOrDefault(x => x.Identity == changedVariable.Identity);

                Assert.That(upsertedEntity, Is.Not.Null);
                Assert.AreEqual(changedVariable.NewValue, upsertedEntity.NewValue);
            }
        }

        [Test]
        public void when_update_answer()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questions = new[]
            {
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)), Answer = (object)1},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)), Answer = (object)"string"},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)), Answer = (object)111.11m},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4)), Answer = (object)2222L},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4,5)), Answer = (object)new[]{new[]{1,2,3}, new []{1,2}}},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4,5,6)), Answer = (object)new InterviewTextListAnswers(new []{new Tuple<decimal, string>(1, "list 1"), })},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4,5,6,7)), Answer = (object)new []{new AnsweredYesNoOption(12, true), new AnsweredYesNoOption(1,false) }},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4,5,6)), Answer = (object)new GeoPosition{Accuracy = 1, Longitude = 2, Latitude = 3, Altitude = 4, Timestamp = DateTimeOffset.Now}},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4,5)), Answer = (object)AudioAnswer.FromString("path/to/file.avi", TimeSpan.FromSeconds(2))},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3,4)), Answer = (object)new Area("geometry", "map", 1, 1, "1:1", 1)},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)), Answer = (object)new DateTime(2012,12,12)},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)), Answer = (object)new[]{1,2,3}},
                new {Id = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)), Answer = (object)true}
            };

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() =>
            {
                foreach (var question in questions)
                    factory.UpdateAnswer(interviewId, question.Id, question.Answer);
            });

            //assert
            var upsertedEntities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider
                .GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn}, {AsStringColumn}, " +
                    $"{AsStringColumn}, {AsIntColumn}, {AsDoubleColumn}, {AsBoolColumn}, {AsDateTimeColumn}, {AsLongColumn}, {AsIntArrayColumn}, " +
                    $"{AsListColumn}, {AsGpsColumn}, {AsYesNoColumn}, {AsAudioColumn}, {AsAreaColumn}, {AsIntMatrixColumn} " +
                    $"FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                    new {InterviewId = interviewId})
                .Select(x => new
                {
                    Id = Identity.Create((Guid) x.entityid, (int[]) x.rostervector),
                    Answer = x.asstring ?? x.asint ?? x.asdouble ?? x.aslong ?? x.asdatetime ?? x.asintarray ??  x.asbool ?? 
                    (x.aslist != null ? new EntitySerializer<InterviewTextListAnswers>().Deserialize(x.aslist) : null ??  
                    x.asintmatrix != null ? new EntitySerializer<int[][]>().Deserialize(x.asintmatrix) : null ??
                    x.asyesno != null ? new EntitySerializer<AnsweredYesNoOption[]>().Deserialize(x.asyesno) : null ??
                    x.asgps != null ? new EntitySerializer<GeoPosition>().Deserialize(x.asgps) : null ??
                    x.asaudio != null ? new EntitySerializer<AudioAnswer>().Deserialize(x.asaudio) : null ??
                    x.asarea != null ? new EntitySerializer<Area>().Deserialize(x.asarea) : null)
                })
                .ToArray());

            Assert.That(upsertedEntities.Length, Is.EqualTo(13));
            foreach (var question in questions)
            {
                var upsertedEntity = upsertedEntities.FirstOrDefault(x => x.Id == question.Id);

                Assert.That(upsertedEntity, Is.Not.Null);
                Assert.AreEqual(question.Answer, upsertedEntity.Answer);
            }
        }

        [Test]
        public void when_remove_answers()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var removedAnswersByIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                Identity.Create(Guid.NewGuid(), Create.RosterVector())
            };

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, " +
                    $"{AsStringColumn}, {AsIntColumn}, {AsDoubleColumn}, {AsBoolColumn}, {AsDateTimeColumn}, {AsLongColumn}, {AsIntArrayColumn}, " +
                    $"{AsListColumn}, {AsGpsColumn}, {AsYesNoColumn}, {AsAudioColumn}, {AsAreaColumn}, {AsIntMatrixColumn}) " +
                    "VALUES(@InterviewId, @EntityId, @RosterVector, 2, 'some string', 123, 543.33, true, '12.12.1912', 43332221, '{1,2,3}', " +
                    "'[{\"Value\": 1.0, \"Answer\": \"List answer 1\"}, {\"Value\": 2.0, \"Answer\": \"List answer 2\"}, {\"Value\": 3.0, \"Answer\": \"List answer 3\"}]', " +
                    "'{\"Accuracy\": 20.0, \"Altitude\": 149.0, \"Latitude\": 49.9961549, \"Longitude\": 36.2351184, \"Timestamp\": \"2017-06-15T02:31:30.617+03:00\"}', " +
                    "'[{\"Yes\": true, \"OptionValue\": 30.0}, {\"Yes\": true, \"OptionValue\": 1.0}, {\"OptionValue\": 10.0}]', " +
                    "'{\"Length\": \"00:00:01.3650000\", \"FileName\": \"audio10__.m4a\"}', " +
                    "'{\"Length\": 2879542.4219173165, \"MapName\": \"WorldMap\", \"AreaSize\": 184623229491.99097, \"Geometry\": \"{\\\"rings\\\":[[[2635991.5671534063,1386582.0326643158],[3664965.4209963419,846849.95737624168],[3174886.9280785033,940524.48176060524],[3115670.4648271999,601827.74066906702],[2635991.5671534063,1386582.0326643158]]],\\\"spatialReference\\\":{\\\"wkid\\\":102100,\\\"latestWkid\\\":3857}}\", \"Coordinates\": \"23.6795151358407,12.3589088764445;32.9229445345055,7.58512897451821;28.520494528442,8.41842093372743;27.9885439883405,5.39830594958836\"}', " +
                    "'[[10], [3000], [40000]]') ",
                    removedAnswersByIdentities.Select(x => new
                    {
                        InterviewId = interviewId,
                        EntityId = x.Id,
                        RosterVector = x.RosterVector.Array
                    })));

            var factory = CreateInterviewFactory();

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.RemoveAnswers(interviewId, removedAnswersByIdentities));

            //assert
            var upsertedIdentities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId " +
                    $"AND ({AsGpsColumn} IS NOT NULL " +
                    $"OR {AsStringColumn} IS NOT NULL " +
                    $"OR {AsIntColumn} IS NOT NULL " +
                    $"OR {AsIntArrayColumn} IS NOT NULL " +
                    $"OR {AsIntMatrixColumn} IS NOT NULL " +
                    $"OR {AsYesNoColumn} IS NOT NULL " +
                    $"OR {AsBoolColumn} IS NOT NULL " +
                    $"OR {AsDateTimeColumn} IS NOT NULL " +
                    $"OR {AsDoubleColumn} IS NOT NULL " +
                    $"OR {AsAreaColumn} IS NOT NULL " +
                    $"OR {AsAudioColumn} IS NOT NULL " +
                    $"OR {AsListColumn} IS NOT NULL " +
                    $"OR {AsLongColumn} IS NOT NULL) ",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .ToArray());

            Assert.That(upsertedIdentities, Is.Empty);
        }

        [Test]
        public void when_getting_all_enabled_multimedia_answers()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var expectedMultimediaAnswers = new[]
            {
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 1", InterviewId = interviewId},
                    Enabled = true
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 2", InterviewId = interviewId},
                    Enabled = true
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 3", InterviewId = Guid.NewGuid()},
                    Enabled = false
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                    Answer = new InterviewStringAnswer {Answer = "path to photo 4", InterviewId = Guid.NewGuid()},
                    Enabled = true
                },
            };

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {EnabledColumn}, {AsStringColumn}) " +
                    $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Enabled, @Answer) ",
                    expectedMultimediaAnswers.Select(x=>new
                    {
                        InterviewId = x.Answer.InterviewId,
                        EntityId = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector.Array,
                        EntityType = EntityType.Question,
                        Enabled = x.Enabled,
                        Answer = x.Answer.Answer
                    })));


            var factory = CreateInterviewFactory();

            //act
            var allMultimediaAnswers = this.sessionProvider.ExecuteInPlainTransaction(
                () => factory.GetAllMultimediaAnswers(expectedMultimediaAnswers.Select(x => x.QuestionId.Id).ToArray()));

            //assert
            Assert.That(allMultimediaAnswers.Length, Is.EqualTo(3));
            Assert.That(allMultimediaAnswers, Is.EquivalentTo(expectedMultimediaAnswers.Where(x=>x.Enabled).Select(x=>x.Answer)));
        }

        [Test]
        public void when_getting_all_enabled_audio_answers()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var expectedAudioAnswers = new[]
            {
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 1", InterviewId = interviewId},
                    Enabled = true
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 2", InterviewId = interviewId},
                    Enabled = true
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 3", InterviewId = Guid.NewGuid()},
                    Enabled = false
                },
                new
                {
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2,3)),
                    Answer = new InterviewStringAnswer {Answer = "path to audio 4", InterviewId = Guid.NewGuid()},
                    Enabled = true
                },
            };

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {EnabledColumn}, {AsAudioColumn}) " +
                    $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Enabled, @Answer) ",
                    expectedAudioAnswers.Select(x => new
                    {
                        InterviewId = x.Answer.InterviewId,
                        EntityId = x.QuestionId.Id,
                        RosterVector = x.QuestionId.RosterVector.Array,
                        EntityType = EntityType.Question,
                        Enabled = x.Enabled,
                        Answer = AudioAnswer.FromString(x.Answer.Answer, new TimeSpan(1, 1, 1, 1, DateTime.Now.Millisecond))
                    })));


            var factory = CreateInterviewFactory();

            //act
            var allAudioAnswers = this.sessionProvider.ExecuteInPlainTransaction(() => factory.GetAllAudioAnswers());

            //assert
            Assert.That(allAudioAnswers.Length, Is.EqualTo(3));
            Assert.That(allAudioAnswers, Is.EquivalentTo(expectedAudioAnswers.Where(x => x.Enabled).Select(x => x.Answer)));
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
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1)),
                    Answer = new GeoPosition{Longitude = 2, Latitude = 2, Accuracy = 2, Altitude = 2, Timestamp = DateTimeOffset.Now}
                },
                new
                {
                    InterviewId = Guid.NewGuid(),
                    QuestionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1,2)),
                    Answer = new GeoPosition{Longitude = 3, Latitude = 3, Accuracy = 3, Altitude = 3, Timestamp = DateTimeOffset.Now}
                }
            };



            this.sessionProvider.ExecuteInPlainTransaction(() =>
            {
                this.sessionProvider.GetSession().Connection.Execute("INSERT INTO readside.interviewsummaries VALUES(@InterviewId,@InterviewId,'Questionnaire title','responsible','9d003a6a-b479-4809-bf31-307871aeed93','responsible',4,'2017-06-28 03:34:17',FALSE,FALSE,FALSE,'6158dd07-4d64-498f-8a50-e5e9828fda23',1,'0ba7b65d-2107-4b43-ba01-b4aac0a973a8',60,FALSE,TRUE,@ClientKey,9458,'',@QuestionnaireId,FALSE)",
                    expectedGpsAnswers.Select(x=>x.InterviewId).Distinct().Select(x=>new {InterviewId = x, QuestionnaireId = questionnaireId.ToString(), ClientKey = x.ToString().Substring(0, 12)}));

                this.sessionProvider.GetSession().Connection
                    .Execute(
                        $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AsGpsColumn}) " +
                        $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Answer) ",
                        expectedGpsAnswers.Select(x => new
                        {
                            InterviewId = x.InterviewId,
                            EntityId = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector.Array,
                            EntityType = EntityType.Question,
                            Answer = x.Answer
                        }));
            });


            var factory = CreateInterviewFactory();

            //act
            var allGpsQuestionIds = this.sessionProvider.ExecuteInPlainTransaction(() => factory.GetAnsweredGpsQuestionIdsByQuestionnaire(questionnaireId));

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
            
            this.sessionProvider.ExecuteInPlainTransaction(() =>
            {
                this.sessionProvider.GetSession().Connection.Execute($"DELETE FROM {InterviewsTableName}");

                this.sessionProvider.GetSession().Connection.Execute("INSERT INTO readside.interviewsummaries VALUES(@InterviewId,@InterviewId,'Questionnaire title','responsible','9d003a6a-b479-4809-bf31-307871aeed93','responsible',4,'2017-06-28 03:34:17',FALSE,FALSE,FALSE,'6158dd07-4d64-498f-8a50-e5e9828fda23',1,'0ba7b65d-2107-4b43-ba01-b4aac0a973a8',60,FALSE,TRUE,@ClientKey,9458,'',@QuestionnaireId,FALSE)",
                    expectedGpsAnswers.Select(x => new { InterviewId = x.InterviewId, QuestionnaireId = x.QuestionnaireId.ToString(), ClientKey = x.InterviewId.ToString().Substring(0,12) }));

                this.sessionProvider.GetSession().Connection
                    .Execute(
                        $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AsGpsColumn}) " +
                        $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Answer) ",
                        expectedGpsAnswers.Select(x => new
                        {
                            InterviewId = x.InterviewId,
                            EntityId = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector.Array,
                            EntityType = EntityType.Question,
                            Answer = x.Answer
                        }));
            });


            var factory = CreateInterviewFactory();

            //act
            var questionnaireIdentities = this.sessionProvider.ExecuteInPlainTransaction(() => factory.GetQuestionnairesWithAnsweredGpsQuestions());

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

            this.sessionProvider.ExecuteInPlainTransaction(() =>
            {
                this.sessionProvider.GetSession().Connection.Execute("INSERT INTO readside.interviewsummaries VALUES(@InterviewId,@InterviewId,'Questionnaire title','responsible','9d003a6a-b479-4809-bf31-307871aeed93','responsible',4,'2017-06-28 03:34:17',FALSE,FALSE,FALSE,'6158dd07-4d64-498f-8a50-e5e9828fda23',1,'0ba7b65d-2107-4b43-ba01-b4aac0a973a8',60,FALSE,TRUE,@ClientKey,9458,'',@QuestionnaireId,FALSE)",
                    allGpsAnswers.Select(x => new { InterviewId = x.InterviewId, QuestionnaireId = x.QuestionnaireId.ToString(), ClientKey = x.InterviewId.ToString().Substring(0, 12) }));

                this.sessionProvider.GetSession().Connection
                    .Execute(
                        $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AsGpsColumn}) " +
                        $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Answer) ",
                        allGpsAnswers.Select(x => new
                        {
                            InterviewId = x.InterviewId,
                            EntityId = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector.Array,
                            EntityType = EntityType.Question,
                            Answer = x.Answer
                        }));
            });


            var factory = CreateInterviewFactory();

            //act
            var gpsAnswers = this.sessionProvider.ExecuteInPlainTransaction(
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

            this.sessionProvider.ExecuteInPlainTransaction(() =>
            {
                this.sessionProvider.GetSession().Connection.Execute("INSERT INTO readside.interviewsummaries VALUES(@InterviewId,@InterviewId,'Questionnaire title','responsible','9d003a6a-b479-4809-bf31-307871aeed93','responsible',4,'2017-06-28 03:34:17',FALSE,FALSE,FALSE,'6158dd07-4d64-498f-8a50-e5e9828fda23',1,'0ba7b65d-2107-4b43-ba01-b4aac0a973a8',60,FALSE,TRUE,@ClientKey,9458,'',@QuestionnaireId,FALSE)",
                    allGpsAnswers.Select(x => new { InterviewId = x.InterviewId, QuestionnaireId = x.QuestionnaireId.ToString(), ClientKey = x.InterviewId.ToString().Substring(0, 12) }));

                this.sessionProvider.GetSession().Connection
                    .Execute(
                        $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AsGpsColumn}) " +
                        $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Answer) ",
                        allGpsAnswers.Select(x => new
                        {
                            InterviewId = x.InterviewId,
                            EntityId = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector.Array,
                            EntityType = EntityType.Question,
                            Answer = x.Answer
                        }));
            });


            var factory = CreateInterviewFactory();

            //act
            var gpsAnswers = this.sessionProvider.ExecuteInPlainTransaction(
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

            this.sessionProvider.ExecuteInPlainTransaction(() =>
            {
                this.sessionProvider.GetSession().Connection.Execute("INSERT INTO readside.interviewsummaries VALUES(@InterviewId,@InterviewId,'Questionnaire title','responsible','9d003a6a-b479-4809-bf31-307871aeed93','responsible',4,'2017-06-28 03:34:17',FALSE,FALSE,FALSE,'6158dd07-4d64-498f-8a50-e5e9828fda23',1,'0ba7b65d-2107-4b43-ba01-b4aac0a973a8',60,FALSE,TRUE,@ClientKey,9458,'',@QuestionnaireId,FALSE)",
                    allGpsAnswers.Select(x => new { InterviewId = x.InterviewId, QuestionnaireId = x.QuestionnaireId.ToString(), ClientKey = x.InterviewId.ToString().Substring(0, 12) }));

                this.sessionProvider.GetSession().Connection
                    .Execute(
                        $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}, {AsGpsColumn}) " +
                        $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType, @Answer) ",
                        allGpsAnswers.Select(x => new
                        {
                            InterviewId = x.InterviewId,
                            EntityId = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector.Array,
                            EntityType = EntityType.Question,
                            Answer = x.Answer
                        }));
            });


            var factory = CreateInterviewFactory();

            //act
            var gpsAnswers = this.sessionProvider.ExecuteInPlainTransaction(
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

            this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection
                .Execute(
                    $"INSERT INTO {InterviewsTableName} ({InterviewIdColumn}, {EntityIdColumn}, {RosterVectorColumn}, {EntityTypeColumn}) " +
                    $"VALUES(@InterviewId, @EntityId, @RosterVector, @EntityType) ",
                    questionnaire.Children[0].Children.SelectMany(x => x.Children).Select(x =>
                        new
                        {
                            InterviewId = interviewId,
                            EntityId = x.PublicKey,
                            RosterVector = rosterVector.Array,
                            EntityType = x is Group ? EntityType.Section : (x is StaticText ? EntityType.StaticText : (x is Variable ? EntityType.Variable : EntityType.Question))
                        })));


            var factory = CreateInterviewFactory(questionnaireStorage: questionnaireStorage);

            //act
            this.sessionProvider.ExecuteInPlainTransaction(() => factory.RemoveRosters(questionnaireId, interviewId,
                questionnaire.Children[0].Children.Select(x => Identity.Create(x.PublicKey, rosterVector)).ToArray()));

            //assert
            var interviewEntities = this.sessionProvider.ExecuteInPlainTransaction(() => this.sessionProvider.GetSession().Connection.Query(
                    $"SELECT {EntityIdColumn}, {RosterVectorColumn} FROM {InterviewsTableName} WHERE {InterviewIdColumn} = @InterviewId",
                    new { InterviewId = interviewId })
                .Select(x => Identity.Create((Guid)x.entityid, (int[])x.rostervector))
                .ToArray());

            Assert.That(interviewEntities, Is.Empty);
        }
    }

    public class JsonHandler<T> : SqlMapper.TypeHandler<T> where T: class
    {
        public static readonly JsonHandler<T> Instance = new JsonHandler<T>();
        
        public override T Parse(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return default(T);
            }
            return new EntitySerializer<T>().Deserialize((string)value);
        }
        public override void SetValue(IDbDataParameter parameter, T value)
        {
            var npgsqlParameter = parameter as NpgsqlParameter;
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;

            if (value == null)
            {
                npgsqlParameter.Value = DBNull.Value;
            }
            else
            {
                npgsqlParameter.Value = new EntitySerializer<T>().Serialize(value);
            }
        }
    }
}