using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewFactoryTests
{
    [TestOf(typeof(InterviewFactory))]
    internal class InterviewFactoryTests : InterviewFactorySpecification
    {
        protected void StoreQuestionnaireDocument(QuestionnaireDocument document)
        {
            //new HqQuestionnaireStorage()
        }

        [Test]
        public void KP_11251_when_there_is_roster_with_negative_value()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 1);
            var rosterSource = Id.g1;
            var roster = Id.g2;
            var rosterItem = Id.g3;

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                interviewSummaryRepository.Store(new InterviewSummary
                {
                    SummaryId = interviewId.FormatGuid(),
                    InterviewId = interviewId,
                    Status = InterviewStatus.Completed,
                    QuestionnaireIdentity = questionnaireId.ToString(),
                    ReceivedByInterviewer = false
                }, interviewId.FormatGuid()));

            var factory = CreateInterviewFactory();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,

                children: new IComposite[]
                {
                    Create.Entity.MultyOptionsQuestion(rosterSource, new List<Answer> {
                        Create.Entity.Answer("Yep", 1),
                        Create.Entity.Answer("Idontknow!", -99)
                    }),
                    Create.Entity.Roster(roster, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSource,
                        children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(rosterItem)
                        })
                });

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);

            var entityNegative = Create.Identity(rosterItem, Create.RosterVector(-99));
            var entityNegative2 = Create.Identity(rosterItem, Create.RosterVector(1, -99));
            var entityNegativeMixed = Create.Identity(rosterItem, Create.RosterVector(1, -99, 3));
            var entityNegativeConsecutive = Create.Identity(rosterItem, Create.RosterVector(1, -99, 3, -99, -44));


            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            factory.Save(new InterviewState
            {
                Id = interviewId,
                Enablement = new Dictionary<InterviewStateIdentity, bool>
                {
                    {InterviewStateIdentity.Create(entityNegative), true},
                    {InterviewStateIdentity.Create(entityNegative2), true},
                    {InterviewStateIdentity.Create(entityNegativeMixed), true},
                    {InterviewStateIdentity.Create(entityNegativeConsecutive), true}
                }
            }));

            //act
            var entities = this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                factory.GetInterviewEntities(interviewId));

            //assert
            Assert.That(entities, Has.Count.EqualTo(4));

            Assert.That(entities, Has.One.Property(nameof(InterviewEntity.Identity)).EqualTo(entityNegative));
            Assert.That(entities, Has.One.Property(nameof(InterviewEntity.Identity)).EqualTo(entityNegative2));
            Assert.That(entities, Has.One.Property(nameof(InterviewEntity.Identity)).EqualTo(entityNegativeMixed));
            Assert.That(entities, Has.One.Property(nameof(InterviewEntity.Identity)).EqualTo(entityNegativeConsecutive));
        }

        [Test]
        public void when_getting_flagged_question_ids()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 5);

            var questionIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1))
            };

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                interviewSummaryRepository.Store(new InterviewSummary
                {
                    SummaryId = interviewId.FormatGuid(),
                    InterviewId = interviewId,
                    Status = InterviewStatus.Completed,
                    QuestionnaireIdentity = questionnaireId.ToString(),
                    ReceivedByInterviewer = false
                }, interviewId.FormatGuid()));

            var factory = CreateInterviewFactory();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,

                children: new IComposite[]
                {
                        Create.Entity.TextQuestion(questionIdentities[0].Id),
                        Create.Entity.TextQuestion(questionIdentities[1].Id),
                });

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);

            foreach (var questionIdentity in questionIdentities)
                this.plainTransactionManager.ExecuteInPlainTransaction(()
                    => factory.SetFlagToQuestion(interviewId, questionIdentity, true));

            //act
            var flaggedIdentites = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetFlaggedQuestionIds(interviewId));

            //assert
            Assert.AreEqual(2, flaggedIdentites.Length);
            Assert.That(flaggedIdentites, Is.EquivalentTo(questionIdentities));
        }

        [Test]
        public void when_setting_flags_should_get_proper_one()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 5);

            var questionIdentities = new[]
            {
                Identity.Create(Guid.NewGuid(), Create.RosterVector()),
                Identity.Create(Guid.NewGuid(), Create.RosterVector(1))
            };

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
                interviewSummaryRepository.Store(new InterviewSummary
                {
                    SummaryId = interviewId.FormatGuid(),
                    InterviewId = interviewId,
                    Status = InterviewStatus.Completed,
                    QuestionnaireIdentity = questionnaireId.ToString(),
                    ReceivedByInterviewer = false
                }, interviewId.FormatGuid()));

            var factory = CreateInterviewFactory();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,

                children: new IComposite[]
                {
                        Create.Entity.TextQuestion(questionIdentities[0].Id),
                        Create.Entity.TextQuestion(questionIdentities[1].Id),
                });

            PrepareQuestionnaire(questionnaire, questionnaireId.Version);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var questionIdentity in questionIdentities)
                {
                    factory.SetFlagToQuestion(interviewId, questionIdentity, true);
                }
            });

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.SetFlagToQuestion(interviewId, questionIdentities[0], false);
            });

            //act
            var flaggedIdentites = this.plainTransactionManager.ExecuteInPlainTransaction(() => factory.GetFlaggedQuestionIds(interviewId));

            //assert
            Assert.AreEqual(1, flaggedIdentites.Length);
            Assert.That(flaggedIdentites, Is.EquivalentTo(new[] { questionIdentities[1] }));
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

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

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

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            var interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.ReadOnly = readOnlyQuestions.Select(InterviewStateIdentity.Create).ToList();

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

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

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

            Assert.That(interviewEntities.Length, Is.EqualTo(4));
            Assert.That(new[] { sectionId, questionId, staticTextId, variableId }, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
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

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

            Assert.That(interviewEntities.Length, Is.EqualTo(3));
            Assert.That(new[] { questionId, staticTextId, variableId }, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
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

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(new[] { questionId, staticTextId }, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
            Assert.That(interviewState.Validity.Values.Select(x => x.Validations), Is.EquivalentTo(interviewEntities.Select(x => x.InvalidValidations)));
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

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRoster(addedRosterIdentities[0].Id),
                Create.Entity.NumericRoster(addedRosterIdentities[1].Id));

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var interviewState = Create.Entity.InterviewState(interviewId);
                interviewState.Enablement = addedRosterIdentities.ToDictionary(InterviewStateIdentity.Create, x => true);

                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

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
                 new InterviewEntity{EntityType = EntityType.Question, AsArea = new Area("geometry", "map", 3, 1, 1, "1:1", 1)},
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

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questions.Select(ToQuestionnaireEntity).ToArray());

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
                    AsAudio = x.AsAudio,
                    AsGps = x.AsGps,
                    AsArea = x.AsArea,
                    AsList = x.AsList,
                    AsIntMatrix = x.AsIntMatrix,
                    AsYesNo = x.AsYesNo
                });

            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

            Assert.That(interviewEntities.Length, Is.EqualTo(16));

            foreach (var expectedQuestion in questions)
            {
                var actualQuestion = interviewEntities.Find(x =>
                    x.InterviewId == expectedQuestion.InterviewId && x.Identity == expectedQuestion.Identity);

                Assert.That(actualQuestion.EntityType, Is.EqualTo(expectedQuestion.EntityType));
                Assert.That(actualQuestion.AsString, Is.EqualTo(expectedQuestion.AsString));
                Assert.That(actualQuestion.AsBool, Is.EqualTo(expectedQuestion.AsBool));
                Assert.That(actualQuestion.AsIntArray, Is.EqualTo(expectedQuestion.AsIntArray));
                Assert.That(actualQuestion.AsDateTime, Is.EqualTo(expectedQuestion.AsDateTime));
                Assert.That(actualQuestion.AsDouble, Is.EqualTo(expectedQuestion.AsDouble));
                Assert.That(actualQuestion.AsInt, Is.EqualTo(expectedQuestion.AsInt));
                Assert.That(actualQuestion.AsLong, Is.EqualTo(expectedQuestion.AsLong));

                Assert.That(JsonConvert.SerializeObject(actualQuestion.AsArea), Is.EqualTo(JsonConvert.SerializeObject(expectedQuestion.AsArea)));
                Assert.That(JsonConvert.SerializeObject(actualQuestion.AsGps), Is.EqualTo(JsonConvert.SerializeObject(expectedQuestion.AsGps)));
                Assert.That(JsonConvert.SerializeObject(actualQuestion.AsIntMatrix), Is.EqualTo(JsonConvert.SerializeObject(expectedQuestion.AsIntMatrix)));
                Assert.That(JsonConvert.SerializeObject(actualQuestion.AsList), Is.EqualTo(JsonConvert.SerializeObject(expectedQuestion.AsList)));
                Assert.That(JsonConvert.SerializeObject(actualQuestion.AsYesNo), Is.EqualTo(JsonConvert.SerializeObject(expectedQuestion.AsYesNo)));
                Assert.That(JsonConvert.SerializeObject(actualQuestion.AsAudio), Is.EqualTo(JsonConvert.SerializeObject(expectedQuestion.AsAudio)));
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
                 new InterviewEntity{EntityType = EntityType.Question, AsArea = new Area("geometry", "map", 3, 1, 1, "1:1", 1)},
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

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questions.Select(ToQuestionnaireEntity).ToArray());

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
                    AsAudio = x.AsAudio,
                    AsGps = x.AsGps,
                    AsArea = x.AsArea,
                    AsList = x.AsList,
                    AsIntMatrix = x.AsIntMatrix,
                    AsYesNo = x.AsYesNo
                });

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            interviewState = Create.Entity.InterviewState(interviewId);
            interviewState.Answers = questions.ToDictionary(x => InterviewStateIdentity.Create(x.Identity),
                x => new InterviewStateAnswer { Id = x.Identity.Id, RosterVector = x.Identity.RosterVector });
            //act
            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                factory.Save(interviewState);
            });

            //assert
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

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
            var otherQuestionnaireId = Create.Entity.QuestionnaireIdentity(questionnaireId.QuestionnaireId, 777);

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
                     Enabled = false,
                     QuestionnaireId = questionnaireId
                 },
                 new
                 {
                     QuestionId = InterviewStateIdentity.Create(multimediaQuestionId, Create.RosterVector(1,2)),
                     Answer = new InterviewStringAnswer {Answer = "path to photo 3", InterviewId = Guid.NewGuid()},
                     Enabled = false,
                     QuestionnaireId = otherQuestionnaireId
                 },
                 new
                 {
                     QuestionId = InterviewStateIdentity.Create(multimediaQuestionId, Create.RosterVector(1,2,3)),
                     Answer = new InterviewStringAnswer {Answer = "path to photo 4", InterviewId = Guid.NewGuid()},
                     Enabled = true,
                     QuestionnaireId = questionnaireId
                 },
             };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,
                children: Create.Entity.MultimediaQuestion(multimediaQuestionId));

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire, questionnaireId.Version);
            PrepareQuestionnaire(questionnaire, otherQuestionnaireId.Version);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var expectedMultimediaAnswer in expectedMultimediaAnswers.GroupBy(x => x.Answer.InterviewId))
                {
                    var id = expectedMultimediaAnswer.Key;

                    interviewSummaryRepository.Store(new InterviewSummary
                    {
                        InterviewId = id,
                        Status = InterviewStatus.Completed,
                        ReceivedByInterviewer = false,
                        QuestionnaireIdentity = expectedMultimediaAnswer.FirstOrDefault().QuestionnaireId.ToString()
                    }, id.FormatGuid());
                }
            });

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in expectedMultimediaAnswers.GroupBy(x => x.Answer.InterviewId))
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
                () => factory.GetMultimediaAnswersByQuestionnaire(questionnaireId));

            //assert
            Assert.That(allMultimediaAnswers.Length, Is.EqualTo(2));
            Assert.That(allMultimediaAnswers, Is.EquivalentTo(expectedMultimediaAnswers
                .Where(x => x.QuestionnaireId == questionnaireId && x.Enabled).Select(x => x.Answer)));
        }

        [Test]
        public void when_getting_all_enabled_audio_answers_by_questionnaire()
        {
            //arrange
            var interviewId = Guid.NewGuid();
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 55);
            var otherQuestionnaireId = new QuestionnaireIdentity(questionnaireId.QuestionnaireId, 777);
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
                     QuestionnaireId = otherQuestionnaireId
                 },
                 new
                 {
                     QuestionId = InterviewStateIdentity.Create(audioQuestionId, Create.RosterVector(1,2,3)),
                     Answer = new InterviewStringAnswer {Answer = "path to audio 4", InterviewId = Guid.NewGuid()},
                     Enabled = true,
                     QuestionnaireId = questionnaireId
                 },
             };

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId.QuestionnaireId,
                children: Create.Entity.AudioQuestion(audioQuestionId, "myaudio"));

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire, questionnaireId.Version);
            PrepareQuestionnaire(questionnaire, otherQuestionnaireId.Version);

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
            });

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var groupedInterviews in expectedAudioAnswers.GroupBy(x => x.Answer.InterviewId))
                {
                    var interviewState = Create.Entity.InterviewState(groupedInterviews.Key);
                    interviewState.Answers = groupedInterviews.ToDictionary(x => x.QuestionId,
                        x => new InterviewStateAnswer
                        {
                            Id = x.QuestionId.Id,
                            RosterVector = x.QuestionId.RosterVector,
                            AsAudio = AudioAnswer.FromString(x.Answer.Answer, TimeSpan.FromMinutes(2))
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
        public void when_make_entities_with_warnings()
        {
            //arrange
            var interviewId = Guid.NewGuid();

            var questionId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2));
            var staticTextId = Identity.Create(Guid.NewGuid(), Create.RosterVector(1, 2, 3));
            var questionnaireId = new QuestionnaireIdentity(Guid.NewGuid(), 1);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                null, questionnaireId.QuestionnaireId,
                    Create.Entity.StaticText(staticTextId.Id),
                    Create.Entity.TextQuestion(questionId.Id));

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire, questionnaireId.Version);

            this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                StoreInterviewSummary(new InterviewSummary(questionnaire)
                {
                    InterviewId = interviewId,
                    QuestionnaireIdentity = questionnaireId.ToString()
                }, questionnaireId);
            });

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
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

            Assert.That(interviewEntities.Length, Is.EqualTo(2));
            Assert.That(new[] { questionId, staticTextId }, Is.EquivalentTo(interviewEntities.Select(x => x.Identity)));
            Assert.That(interviewState.Warnings.Values.Select(x => x.Validations), Is.EquivalentTo(interviewEntities.Select(x => x.WarningValidations)));
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

            var factory = CreateInterviewFactory();
            PrepareQuestionnaire(questionnaire);
            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

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
            var interviewEntities = this.GetInterviewEntities(factory, interviewId, questionnaire.PublicKey);

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
    }
}
