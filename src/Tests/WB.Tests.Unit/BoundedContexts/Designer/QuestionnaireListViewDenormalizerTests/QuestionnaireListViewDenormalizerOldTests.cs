extern alias designer;

using System;
using System.Linq;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    [TestFixture]
    internal class QuestionnaireListViewDenormalizerOldTests
    {
        [Test]
        public void Handle_When_QuestionnaireCloned_event_received()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();

            Guid questionnaireId = Guid.NewGuid();
            string newtitle = "newTitle";
            var document = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle, CreatedBy = Guid.NewGuid()};

            TestInMemoryWriter<QuestionnaireListViewItem> questionnaireStorage = new TestInMemoryWriter<QuestionnaireListViewItem>();

            QuestionnaireListViewItemDenormalizer target = CreateQuestionnaireDenormalizer(questionnaireStorage: questionnaireStorage);
            var eventToPublish = CreateEvent(new QuestionnaireCloned() { QuestionnaireDocument = document }, DateTime.Now);
            // act
            target.Handle(eventToPublish);

            // assert
            var storedQuestionnaire = questionnaireStorage.Dictionary.Values.FirstOrDefault(i =>
                    i.Title == newtitle && i.CreationDate == eventToPublish.EventTimeStamp &&
                    i.LastEntryDate == eventToPublish.EventTimeStamp);

            Assert.That(storedQuestionnaire, Is.Not.Null);
        }

        [Test]
        public void Handle_When_TemplateImported_event_received()
        {
            // arrange
            AssemblyContext.SetupServiceLocator();

            Guid questionnaireId = Guid.NewGuid();
            string newtitle = "newTitle";
            var documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle, CreationDate = DateTime.Now, LastEntryDate = DateTime.Now };

            var oldView = new QuestionnaireListViewItem(questionnaireId, "test", DateTime.Now, DateTime.Now, null, true);
            oldView.SharedPersons.Add(Guid.NewGuid());
            questionnaireStorageMock.Setup(x => x.GetById(questionnaireId.FormatGuid()))
                .Returns(oldView);
            QuestionnaireListViewItemDenormalizer target = CreateQuestionnaireDenormalizer();
            // act
            target.Handle(CreateEvent(CreateTemplateImportedEvent(documentReplacement)));

            // assert
            questionnaireStorageMock.Verify(
                x =>
                    x.Store(It.Is<QuestionnaireListViewItem>(i => i.Title == newtitle && i.CreationDate == documentReplacement.CreationDate &&
                                    i.LastEntryDate == documentReplacement.CreationDate && i.SharedPersons.Count == 1 &&
                                    i.SharedPersons.Contains(oldView.SharedPersons.First())), questionnaireId.FormatGuid()));
        }


        private TemplateImported CreateTemplateImportedEvent(QuestionnaireDocument content)
        {
            var result = new TemplateImported();
            result.Source = content;
            return result;
        }

        private IPublishedEvent<T> CreateEvent<T>(T payload, DateTime? eventTimestamp=null)
            where T: IEvent
        {
            var mock= new Mock<IPublishedEvent<T>>();
            mock.Setup(x => x.Payload).Returns(payload);
            mock.Setup(x => x.EventTimeStamp).Returns(eventTimestamp ?? DateTime.Now);
            return mock.Object;
        }

        private QuestionnaireListViewItemDenormalizer CreateQuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireListViewItem> questionnaireStorage = null)
        {
            return new QuestionnaireListViewItemDenormalizer(questionnaireStorage ?? questionnaireStorageMock.Object, Mock.Of<IReadSideRepositoryWriter<AccountDocument>>(
                            _ => _.GetById(It.IsAny<string>()) == new AccountDocument() { UserName = "nastya" }));
        }

        private Mock<IReadSideRepositoryWriter<QuestionnaireListViewItem>> questionnaireStorageMock = new Mock<IReadSideRepositoryWriter<QuestionnaireListViewItem>>();
    }
}
