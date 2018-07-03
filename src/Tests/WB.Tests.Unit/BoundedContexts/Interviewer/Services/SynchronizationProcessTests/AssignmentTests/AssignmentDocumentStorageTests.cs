using System.Linq;
using Moq;
using NUnit.Framework;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Tests.Abc;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class AssignmentDocumentStorageTests
    {
        private IAssignmentDocumentsStorage storage;
        private SQLiteConnectionWithLock connection;

        [OneTimeSetUp]
        public void Setup()
        {
            this.connection = Create.Storage.InMemorySqLiteConnection;

            this.storage = new AssignmentDocumentsStorage(connection, Mock.Of<ILogger>());
        }

        [Test]
        public void should_store_answers()
        {
            var document = Create.Entity.AssignmentDocument(1)
                .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                .WithAnswer(Create.Entity.Identity(Id.g3), "IDEN1", true)
                .WithAnswer(Create.Entity.Identity(Id.g4), "IDEN2", true)
                .Build();

            this.storage.Store(document);

            var stored = this.storage.LoadAll().First();

            this.storage.FetchPreloadedData(stored);

            AssertThatStoredAssignmentIsEqualToDocument(stored, document);
        }
        
        [Test]
        public void should_not_load_all_answers_on_loadAll()
        {
            var document = Create.Entity.AssignmentDocument(1)
                .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                .WithAnswer(Create.Entity.Identity(Id.g3), "IDEN1", true)
                .WithAnswer(Create.Entity.Identity(Id.g4), "IDEN1", true)
                .WithAnswer(Create.Entity.Identity(Id.g5), "IDEN3", true)
                .Build();

            this.storage.Store(document);

            var stored = this.storage.LoadAll().First();

            Assert.IsNull(stored.Answers);
            Assert.That(stored.IdentifyingAnswers, Has.Count.EqualTo(3));

            this.storage.FetchPreloadedData(stored);

            Assert.That(stored.Answers, Has.Count.EqualTo(5));
            
            AssertThatStoredAssignmentIsEqualToDocument(stored, document);
        }

        [Test]
        public void should_store_answers_ensuring_that_old_removed_new_added()
        {
            var document = Create.Entity.AssignmentDocument(1)
                .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                .Build();

            this.storage.Store(document);

            document.Answers.Add(new AssignmentDocument.AssignmentAnswer
            {
                Identity = Create.Identity(Id.g3),
                AssignmentId = 1,
                AnswerAsString = "answer3"
            });

            document.Answers.RemoveAt(0);

            this.storage.Store(document);

            var stored = this.storage.LoadAll().First();

            this.storage.FetchPreloadedData(stored);

            AssertThatStoredAssignmentIsEqualToDocument(stored, document);

            var storedAnswers = this.connection.Table<AssignmentDocument.AssignmentAnswer>().ToList();

            Assert.That(storedAnswers, Has.Count.EqualTo(document.Answers.Count), "Ensure that there is no leftover answers in db");
        }

        [Test]
        public void should_be_able_to_removeOne()
        {
            var document = Create.Entity.AssignmentDocument(1)
                    .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                    .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                    .WithAnswer(Create.Entity.Identity(Id.g3), "answer3", identifying: true)
                    .WithAnswer(Create.Entity.Identity(Id.g4), "answer4", identifying: true)
                    .WithAnswer(Create.Entity.Identity(Id.g5), "answer5")
                    .Build();

            this.storage.Store(document);

            this.storage.Remove(1);

            var storedAnswers = this.connection.Table<AssignmentDocument.AssignmentAnswer>().ToList();
            var storedQuestions = this.connection.Table<AssignmentDocument>().ToList();

            Assert.That(storedAnswers, Has.Count.EqualTo(0));
            Assert.That(storedQuestions, Has.Count.EqualTo(0));
        }

        [Test]
        public void should_be_able_to_removeAll()
        {
            var document = Create.Entity.AssignmentDocument(1)
                .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                .WithAnswer(Create.Entity.Identity(Id.g3), "answer3", identifying: true)
                .WithAnswer(Create.Entity.Identity(Id.g4), "answer4")
                .WithAnswer(Create.Entity.Identity(Id.g5), "answer5", identifying: true)
                .Build();

            this.storage.Store(document);

            this.storage.RemoveAll();

            var storedAnswers = this.connection.Table<AssignmentDocument.AssignmentAnswer>().ToList();
            var storedQuestions = this.connection.Table<AssignmentDocument>().ToList();

            Assert.That(storedAnswers, Has.Count.EqualTo(0));
            Assert.That(storedQuestions, Has.Count.EqualTo(0));
        }

        private static void AssertThatStoredAssignmentIsEqualToDocument(AssignmentDocument stored, AssignmentDocument document)
        {
            void AssertEqualAnswers(List<AssignmentDocument.AssignmentAnswer> source, List<AssignmentDocument.AssignmentAnswer> target)
            {
                source = source.OrderBy(s => s.Id).ToList();
                target = target.OrderBy(s => s.Id).ToList();

                source.SequenceEqual(target, answer => answer.AssignmentId, answer => answer.AssignmentId);
                source.SequenceEqual(target, answer => answer.AnswerAsString, answer => answer.AnswerAsString);
                source.SequenceEqual(target, answer => answer.IsIdentifying, answer => answer.IsIdentifying);
                source.SequenceEqual(target, answer => answer.Identity, answer => answer.Identity);
                source.SequenceEqual(target, answer => answer.Question, answer => answer.Question);
                source.SequenceEqual(target, answer => answer.SerializedAnswer, answer => answer.SerializedAnswer);
            }

            AssertEqualAnswers(stored.Answers, document.Answers);
            AssertEqualAnswers(stored.IdentifyingAnswers, stored.IdentifyingAnswers);
            
            Assert.That(stored.LocationLatitude, Is.EqualTo(document.LocationLatitude));
            Assert.That(stored.LocationLongitude, Is.EqualTo(document.LocationLongitude));
            Assert.That(stored.QuestionnaireId, Is.EqualTo(document.QuestionnaireId));
            Assert.That(stored.Quantity, Is.EqualTo(document.Quantity));
            Assert.That(stored.Title, Is.EqualTo(document.Title));
            Assert.That(stored.ReceivedDateUtc, Is.EqualTo(document.ReceivedDateUtc));
        }
    }
}
