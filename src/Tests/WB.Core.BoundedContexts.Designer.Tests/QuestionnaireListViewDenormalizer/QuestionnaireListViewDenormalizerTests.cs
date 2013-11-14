using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireListViewDenormalizer
{
    [TestFixture]
    public class QuestionnaireListViewDenormalizerTests
    {
        [Test]
        public void Handle_When_SnapshotLoaded_event_template_is_absent_Then_new_document_is_added()
        {
            // arrange
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            Guid questionnaireId = Guid.NewGuid();
            string newtitle = "newTitle";
            QuestionnaireDocument documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle };

            var updrader = Mock.Of<IQuestionnaireDocumentUpgrader>(x => x.TranslatePropagatePropertiesToRosterProperties(It.IsAny<QuestionnaireDocument>()) == documentReplacement);

            QuestionnaireListViewItemDenormalizer target = CreateQuestionnaireDenormalizer(updrader);
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

            Guid questionnaireId = Guid.NewGuid();

            QuestionnaireListViewItem currentItem = new QuestionnaireListViewItem(questionnaireId, "title", DateTime.Now, DateTime.Now, null, false);

            string newtitle = "newTitle";
            QuestionnaireDocument documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle};

            var updrader = Mock.Of<IQuestionnaireDocumentUpgrader>(x => x.TranslatePropagatePropertiesToRosterProperties(It.IsAny<QuestionnaireDocument>()) == documentReplacement);

            questionnaireStorageMock.Setup(x => x.GetById(questionnaireId)).Returns(currentItem);

            QuestionnaireListViewItemDenormalizer target = CreateQuestionnaireDenormalizer(updrader);
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

        private QuestionnaireListViewItemDenormalizer CreateQuestionnaireDenormalizer(IQuestionnaireDocumentUpgrader updrader)
        {
            return new QuestionnaireListViewItemDenormalizer(questionnaireStorageMock.Object, accountStorageMock.Object, updrader);
        }

        private Mock<IReadSideRepositoryWriter<QuestionnaireListViewItem>> questionnaireStorageMock = new Mock<IReadSideRepositoryWriter<QuestionnaireListViewItem>>();
        private Mock<IReadSideRepositoryWriter<AccountDocument>> accountStorageMock = new Mock<IReadSideRepositoryWriter<AccountDocument>>();
    }
}
