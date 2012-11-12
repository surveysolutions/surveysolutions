// -----------------------------------------------------------------------
// <copyright file="SyncProcessDenormalizerTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Tests.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.Events.Synchronization;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using NUnit.Framework;

    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class SyncProcessDenormalizerTests
    {
        [Test]
        public void HandleNewSynchronizationProcessCreated_EventIsCome_OneNewItemIsAddedToStorage()
        {
            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new NewSynchronizationProcessCreated
                {
                    ProcessGuid = Guid.NewGuid(),
                    SynckType = SynchronizationType.Push
                };

            IPublishedEvent<NewSynchronizationProcessCreated> e =
                new PublishedEvent<NewSynchronizationProcessCreated>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        evnt.ProcessGuid,
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            storage.Verify(x => x.Store(It.IsAny<SyncProcessStatisticsDocument>(), Guid.Empty), Times.Once());
        }

        [Test]
        public void HandleProcessEnded_EventIsCome_IsEndedSetInTrue()
        {
            var statistics = new SyncProcessStatisticsDocument
            {
                CreationDate = DateTime.Now,
                SyncType = SynchronizationType.Push
            };

            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetByGuid(statistics.PublicKey)).Returns(statistics);

            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new ProcessEnded
            {
                Status = EventState.Completed
            };

            IPublishedEvent<ProcessEnded> e =
                new PublishedEvent<ProcessEnded>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.IsTrue(statistics.IsEnded);
        }

        [Test]
        public void HandleNewCompleteQuestionnaireCreated_CompleteQuestionnaireIsCome_OneRowInStatisticsAdded()
        {
            var doc = new CompleteQuestionnaireDocument
                {
                    PublicKey = Guid.NewGuid(),
                    TemplateId = Guid.NewGuid(),
                    Title = "Title",
                    Responsible = new UserLight(Guid.NewGuid(), "Vasya")
                };
            var question = new SingleCompleteQuestion(string.Empty);
            var answer = new CompleteAnswer(new Answer()) { AnswerValue = "invalid value" };
            question.AddAnswer(answer);
            doc.Children.Add(question);

            var statistics = new SyncProcessStatisticsDocument
            {
                CreationDate = DateTime.Now,
                SyncType = SynchronizationType.Push
            };

            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetByGuid(statistics.PublicKey)).Returns(statistics);

            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new NewCompleteQuestionnaireCreated
            {
                CreationDate = DateTime.Now,
                Questionnaire = doc
            };

            IPublishedEvent<NewCompleteQuestionnaireCreated> e =
                new PublishedEvent<NewCompleteQuestionnaireCreated>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.AreEqual(1, statistics.Statistics.Count);
            Assert.AreEqual(doc.PublicKey, statistics.Statistics[0].SurveyId);
            Assert.AreEqual(doc.TemplateId, statistics.Statistics[0].TemplateId);
            Assert.AreEqual(doc.Title, statistics.Statistics[0].Title);
            Assert.AreEqual(doc.Responsible, statistics.Statistics[0].User);
        }

        [Test]
        public void HandleSnapshootLoaded_CompleteQuestionnaireEventIsCome_OneRowInStatisticsAdded()
        {
            var doc = new CompleteQuestionnaireDocument
            {
                PublicKey = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Title",
                Responsible = new UserLight(Guid.NewGuid(), "Vasya")
            };
            var question = new SingleCompleteQuestion(string.Empty);
            var answer = new CompleteAnswer(new Answer()) { AnswerValue = "invalid value" };
            question.AddAnswer(answer);
            doc.Children.Add(question);

            var statistics = new SyncProcessStatisticsDocument
            {
                CreationDate = DateTime.Now,
                SyncType = SynchronizationType.Push
            };

            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetByGuid(statistics.PublicKey)).Returns(statistics);

            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new SnapshootLoaded { Template = new Snapshot(doc.PublicKey, 1, doc) };

            IPublishedEvent<SnapshootLoaded> e =
                new PublishedEvent<SnapshootLoaded>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.AreEqual(1, statistics.Statistics.Count);
            Assert.AreEqual(doc.PublicKey, statistics.Statistics[0].SurveyId);
            Assert.AreEqual(doc.TemplateId, statistics.Statistics[0].TemplateId);
            Assert.AreEqual(doc.Title, statistics.Statistics[0].Title);
            Assert.AreEqual(doc.Responsible, statistics.Statistics[0].User);
        }

        [Test]
        public void HandleQuestionnaireAssignmentChanged_EventIsCome_OneRowInStatisticsAdded()
        {
            var doc = new CompleteQuestionnaireDocument
            {
                PublicKey = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Title",
                Responsible = new UserLight(Guid.NewGuid(), "Vasya")
            };
            var question = new SingleCompleteQuestion(string.Empty);
            var answer = new CompleteAnswer(new Answer()) { AnswerValue = "invalid value" };
            question.AddAnswer(answer);
            doc.Children.Add(question);

            var statistics = new SyncProcessStatisticsDocument
            {
                CreationDate = DateTime.Now,
                SyncType = SynchronizationType.Push
            };

            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetByGuid(statistics.PublicKey)).Returns(statistics);

            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            survey.Setup(s => s.GetByGuid(doc.PublicKey)).Returns(new CompleteQuestionnaireBrowseItem(doc));

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new QuestionnaireAssignmentChanged
                {
                    CompletedQuestionnaireId = doc.PublicKey,
                    PreviousResponsible = new UserLight(new Guid(), "PrevUser"),
                    Responsible = new UserLight(new Guid(), "User")
                };

            IPublishedEvent<QuestionnaireAssignmentChanged> e =
                new PublishedEvent<QuestionnaireAssignmentChanged>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.AreEqual(1, statistics.Statistics.Count);
            Assert.AreEqual(doc.PublicKey, statistics.Statistics[0].SurveyId);
            Assert.AreEqual(doc.TemplateId, statistics.Statistics[0].TemplateId);
            Assert.AreEqual(doc.Title, statistics.Statistics[0].Title);
            Assert.AreEqual(evnt.Responsible, statistics.Statistics[0].User);
            Assert.AreEqual(evnt.PreviousResponsible, statistics.Statistics[0].PrevUser);
        }

        [Test]
        public void HandleQuestionnaireStatusChanged_EventIsCome_OneRowInStatisticsAdded()
        {
            var doc = new CompleteQuestionnaireDocument
            {
                PublicKey = Guid.NewGuid(),
                TemplateId = Guid.NewGuid(),
                Title = "Title",
                Responsible = new UserLight(Guid.NewGuid(), "Vasya")
            };
            var question = new SingleCompleteQuestion(string.Empty);
            var answer = new CompleteAnswer(new Answer()) { AnswerValue = "invalid value" };
            question.AddAnswer(answer);
            doc.Children.Add(question);

            var statistics = new SyncProcessStatisticsDocument
            {
                CreationDate = DateTime.Now,
                SyncType = SynchronizationType.Push
            };

            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetByGuid(statistics.PublicKey)).Returns(statistics);

            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            survey.Setup(s => s.GetByGuid(doc.PublicKey)).Returns(new CompleteQuestionnaireBrowseItem(doc));

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new QuestionnaireStatusChanged
            {
                CompletedQuestionnaireId = doc.PublicKey,
                Status = SurveyStatus.Complete,
                PreviousStatus = SurveyStatus.Initial,
                Responsible = new UserLight(new Guid(), "User")
            };

            IPublishedEvent<QuestionnaireStatusChanged> e =
                new PublishedEvent<QuestionnaireStatusChanged>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.AreEqual(1, statistics.Statistics.Count);
            Assert.AreEqual(doc.PublicKey, statistics.Statistics[0].SurveyId);
            Assert.AreEqual(doc.TemplateId, statistics.Statistics[0].TemplateId);
            Assert.AreEqual(doc.Title, statistics.Statistics[0].Title);
            Assert.AreEqual(doc.Responsible, statistics.Statistics[0].User);
            Assert.AreEqual(evnt.PreviousStatus, statistics.Statistics[0].PrevStatus);
        }

        [Test]
        public void HandleNewQuestionnaireCreated_EventIsCome_OneRowInStatisticsAdded()
        {
            var statistics = new SyncProcessStatisticsDocument
            {
                CreationDate = DateTime.Now,
                SyncType = SynchronizationType.Push
            };

            var storage = new Mock<IDenormalizerStorage<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetByGuid(statistics.PublicKey)).Returns(statistics);

            var survey = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();

            var denormalizer = new SyncProcessDenormalizer(storage.Object, survey.Object);

            var evnt = new NewQuestionnaireCreated
            {
                CreationDate = DateTime.Now,
                PublicKey = Guid.NewGuid(),
                Title = "Some new title",
            };

            IPublishedEvent<NewQuestionnaireCreated> e =
                new PublishedEvent<NewQuestionnaireCreated>(
                    new UncommittedEvent(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        1,
                        1,
                        DateTime.Now,
                        evnt,
                        new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.AreEqual(1, statistics.Statistics.Count);
            Assert.AreEqual(evnt.PublicKey, statistics.Statistics[0].TemplateId);
        }
    }
}
