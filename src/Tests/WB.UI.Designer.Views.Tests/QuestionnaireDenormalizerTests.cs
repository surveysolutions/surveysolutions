using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.EventHandler;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Views.Tests
{
    [TestFixture]
    public class QuestionnaireDenormalizerTests
    {
        [Test]
        public void Handle_When_SnapshotLoaded_event_template_is_absent_Then_new_document_is_added()
        {
            // arrange
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            QuestionnaireDenormalizer target = CreateQuestionnaireDenormalizer();

            Guid questionnaireId = Guid.NewGuid();
            string newtitle = "newTitle";
            QuestionnaireDocument documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle };

            // act
            target.Handle(CreateEvent(CreateTemplateImportedEvent(documentReplacement)));

            // assert
            questionnaireStorageMock.Verify(
              x => x.Store(It.Is<QuestionnaireListViewItem>(i => i.Title == newtitle), questionnaireId));
            
        }
        [Test]
        public void Handle_When_SnapshotLoadedEventTemplateIsPresent_Then_OldTemplateReplaced()
        {
            // arrange
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            QuestionnaireDenormalizer target = CreateQuestionnaireDenormalizer();

            Guid questionnaireId = Guid.NewGuid();

            QuestionnaireListViewItem currentItem = new QuestionnaireListViewItem(questionnaireId, "title", DateTime.Now,
                                                                                  DateTime.Now, null, false);

            string newtitle = "newTitle";
            QuestionnaireDocument documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle};

            questionnaireStorageMock.Setup(x => x.GetById(questionnaireId)).Returns(currentItem);

            // act
            target.Handle(CreateEvent(CreateTemplateImportedEvent(documentReplacement)));

            // assert
            questionnaireStorageMock.Verify(
                x => x.Store(It.Is<QuestionnaireListViewItem>(i => i.Title == newtitle), questionnaireId));
        }


        private TemplateImported CreateTemplateImportedEvent(QuestionnaireDocument content)
        {
            var result = new TemplateImported();
            result.Source = content;
            return result;
        }

        private IPublishedEvent<T> CreateEvent<T>(T payload)
        {
            var mock= new Mock<IPublishedEvent<T>>();
            mock.Setup(x => x.Payload).Returns(payload);
            
            return mock.Object;
        }

        private QuestionnaireDenormalizer CreateQuestionnaireDenormalizer()
        {
            return new QuestionnaireDenormalizer(questionnaireStorageMock.Object, accountStorageMock.Object);
        }

        private Mock<IReadSideRepositoryWriter<QuestionnaireListViewItem>> questionnaireStorageMock = new Mock<IReadSideRepositoryWriter<QuestionnaireListViewItem>>();
        private Mock<IReadSideRepositoryWriter<AccountDocument>> accountStorageMock = new Mock<IReadSideRepositoryWriter<AccountDocument>>();
    }
}
