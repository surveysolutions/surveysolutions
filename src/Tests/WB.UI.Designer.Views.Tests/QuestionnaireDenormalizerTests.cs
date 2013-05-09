using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Restoring.EventStapshoot;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.EventHandler;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Views.Tests
{
    [TestFixture]
    public class QuestionnaireDenormalizerTests
    {

        [Test]
        public void Handle_When_SnapshotLoaded_event_donot_contains_questionnairie_document_Then_No_actions_was_performed()
        {
            // arrange
            QuestionnaireDenormalizer target = CreateQuestionnaireDenormalizer();

            // act
            target.Handle(CreateEvent(new SnapshootLoaded(){Template = new Snapshot(Guid.NewGuid(),1,new object())}));

            // assert
            questionnaireStorageMock.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never());
            questionnaireStorageMock.Verify(x => x.Store(It.IsAny<QuestionnaireListViewItem>(), It.IsAny<Guid>()),
                                            Times.Never());
        }

        [Test]
        public void Handle_When_SnapshotLoaded_event_template_is_absent_Then_new_document_is_added()
        {
            // arrange
            QuestionnaireDenormalizer target = CreateQuestionnaireDenormalizer();

            Guid questionnaireId = Guid.NewGuid();
            string newtitle = "newTitle";
            QuestionnaireDocument documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle };

            // act
            target.Handle(CreateEvent(CreateSnapshotEvent(documentReplacement)));

            // assert
            questionnaireStorageMock.Verify(
              x => x.Store(It.Is<QuestionnaireListViewItem>(i => i.Title == newtitle), questionnaireId));
            
        }
        [Test]
        public void Handle_When_SnapshotLoadedEventTemplateIsPresent_Then_OldTemplateReplaced()
        {
            // arrange
            QuestionnaireDenormalizer target = CreateQuestionnaireDenormalizer();

            Guid questionnaireId = Guid.NewGuid();

            QuestionnaireListViewItem currentItem = new QuestionnaireListViewItem(questionnaireId, "title", DateTime.Now,
                                                                                  DateTime.Now, null);

            string newtitle = "newTitle";
            QuestionnaireDocument documentReplacement = new QuestionnaireDocument() { PublicKey = questionnaireId, Title = newtitle};

            questionnaireStorageMock.Setup(x => x.GetById(questionnaireId)).Returns(currentItem);

            // act
            target.Handle(CreateEvent(CreateSnapshotEvent(documentReplacement)));

            // assert
            questionnaireStorageMock.Verify(
                x => x.Store(It.Is<QuestionnaireListViewItem>(i => i.Title == newtitle), questionnaireId));
        }


        private SnapshootLoaded CreateSnapshotEvent(QuestionnaireDocument content)
        {
            var result = new SnapshootLoaded();
            result.Template = new Snapshot(content.PublicKey, 1, content);
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

        private Mock<IDenormalizerStorage<QuestionnaireListViewItem>> questionnaireStorageMock=new Mock<IDenormalizerStorage<QuestionnaireListViewItem>>();
        private Mock<IDenormalizerStorage<AccountDocument>> accountStorageMock=new Mock<IDenormalizerStorage<AccountDocument>>();
    }
}
