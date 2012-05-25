using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DataEntryClient.CompleteQuestionnaire;
using DataEntryClient.WcfInfrastructure;
using DataEntryClientTests.Stubs;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Handshake;

namespace DataEntryClientTests
{
    [TestFixture]
    public class CompleteQuestionnaireSyncTest
    {
        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsReturned()
        {
            Mock<ICommandInvoker> invokerMock=new Mock<ICommandInvoker>();
            Mock<IViewRepository> repositoryMock=new Mock<IViewRepository>();

            Mock<IGetLastSyncEvent> serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub<IGetLastSyncEvent>(serviceMock);
          
            

            var clientGuid = Guid.NewGuid();
            var eventGuid = Guid.NewGuid();

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            var target = new CompleteQuestionnaireSync(invokerMock.Object, repositoryMock.Object,
                                                       chanelFactoryStub, clientSettingsMock.Object);

            var result =target.GetLastSyncEventGuid(clientGuid);
            Assert.AreEqual(result, eventGuid);
        }
        [Test]
        public void GetLastEventGuid_ResultNotNull_GuidIsNotReturned()
        {
            Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();

            Mock<IGetLastSyncEvent> serviceMock = new Mock<IGetLastSyncEvent>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub<IGetLastSyncEvent>(serviceMock);



            var clientGuid = Guid.NewGuid();
            Guid? eventGuid = null;

            serviceMock.Setup(x => x.Process(clientGuid)).Returns(eventGuid);
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            var target = new CompleteQuestionnaireSync(invokerMock.Object, repositoryMock.Object,
                                                       chanelFactoryStub, clientSettingsMock.Object);

            var result = target.GetLastSyncEventGuid(clientGuid);
            Assert.AreEqual(result, null);
        }
        [Test]
        public void UploadEvents_2Events_AllEventsAreDelivered()
        {
            Mock<ICommandInvoker> invokerMock = new Mock<ICommandInvoker>();
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();

            Mock<ICompleteQuestionnaireService> serviceMock = new Mock<ICompleteQuestionnaireService>();
            IChanelFactoryWrapper chanelFactoryStub = new ChanelFactoryStub<ICompleteQuestionnaireService>(serviceMock);



            var clientGuid = Guid.NewGuid();
            Guid? eventGuid = Guid.NewGuid();

            serviceMock.Setup(x => x.Process(It.IsAny<EventSyncMessage>())).Returns(ErrorCodes.None);
            repositoryMock.Setup(x => x.Load<EventBrowseInputModel, EventBrowseView>(It.IsAny<EventBrowseInputModel>()))
                .Returns(new EventBrowseView(2, 2, 2,
                                             new List<EventBrowseItem>()
                                                 {
                                                     new EventBrowseItem(Guid.NewGuid(), DateTime.Now, null),
                                                     new EventBrowseItem(Guid.NewGuid(), DateTime.Now, null)
                                                 }));
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            var target = new CompleteQuestionnaireSync(invokerMock.Object, repositoryMock.Object,
                                                       chanelFactoryStub, clientSettingsMock.Object);

            target.UploadEvents(clientGuid,eventGuid);

            serviceMock.Verify(x => x.Process(It.IsAny<EventSyncMessage>()), Times.Exactly(2));

        }
    }
}
