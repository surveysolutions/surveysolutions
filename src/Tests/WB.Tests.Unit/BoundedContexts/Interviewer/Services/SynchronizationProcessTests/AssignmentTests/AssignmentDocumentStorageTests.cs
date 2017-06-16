using System.Linq;
using Moq;
using NUnit.Framework;
using SQLite;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.AssignmentTests
{
    public class AssignmentDocumentStorageTests
    {
        private AssignmentDocumentsStorage storage;
        private SQLiteConnectionWithLock connection;

        [OneTimeSetUp]
        public void Setup()
        {
            this.connection = Create.Storage.InMemorySqLiteConnection;

            this.storage = new AssignmentDocumentsStorage(connection,
                Mock.Of<ILogger>());
        }

        [Test]
        public void should_store_answers()
        {
            var document = Create.Entity.AssignmentDocument(1)
                .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                .Build();

            this.storage.Store(document);

            var stored = this.storage.LoadAll().First();

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

            var stored = this.storage.GetById(1);

            AssertThatStoredAssignmentIsEqualToDocument(stored, document);

            var storedAnswers = this.connection.Table<AssignmentDocument.AssignmentAnswer>().ToList();

            Assert.That(storedAnswers, Has.Count.EqualTo(document.Answers.Count), "Ensure that there is no leftover answers in db");
        }

        [Test]
        public void should_be_able_to_removeAll()
        {
            var document = Create.Entity.AssignmentDocument(1)
                .WithAnswer(Create.Entity.Identity(Id.g1), "answer1")
                .WithAnswer(Create.Entity.Identity(Id.g2), "answer2")
                .WithAnswer(Create.Entity.Identity(Id.g3), "answer3")
                .WithAnswer(Create.Entity.Identity(Id.g4), "answer4")
                .WithAnswer(Create.Entity.Identity(Id.g5), "answer5")
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
            var storedAnswers = stored.Answers.OrderBy(id => id.Id).ToArray();
            var documentAnswers = document.Answers.OrderBy(id => id.Id).ToArray();

            Assert.That(stored.Answers, Has.Count.EqualTo(document.Answers.Count));

            for (int i = 0; i < storedAnswers.Length; i++)
            {
                Assert.That(storedAnswers[i].Identity, Is.EqualTo(documentAnswers[i].Identity));
                Assert.That(storedAnswers[i].AnswerAsString, Is.EqualTo(documentAnswers[i].AnswerAsString));
                Assert.That(storedAnswers[i].AssignmentId, Is.EqualTo(document.Id));
            }
        }
    }
}