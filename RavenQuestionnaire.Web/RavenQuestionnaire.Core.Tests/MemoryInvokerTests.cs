using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using Raven.Client;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.ClientSettings;

namespace RavenQuestionnaire.Core.Tests
{
     [TestFixture]
    public class MemoryInvokerTests
    {
        [Test]
        public void WhenCommandIsSentAndHandlerIsPresent_SessionIsNotFlushed()
        {
            Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            var kernel = new StandardKernel();
            kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            MemoryCommandInvoker invoker = new MemoryCommandInvoker(kernel, clientSettingsMock.Object);
            Mock<ICommand> someCommand = new Mock<ICommand>();

            invoker.Execute(someCommand.Object);
            documentSessionMock.Verify(x => x.SaveChanges(), Times.Never());

        }

        [Test]
        public void WhenCommandIsSentAndHandlerIsPresentButExceptionIsThrown_SessionIsNotFlushed()
        {
            Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
            mockHandler.Setup(x => x.Handle(It.IsAny<ICommand>())).Throws<Exception>();
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            var kernel = new StandardKernel();
            kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            MemoryCommandInvoker invoker = new MemoryCommandInvoker(kernel, clientSettingsMock.Object);
            Mock<ICommand> someCommand = new Mock<ICommand>();

            Assert.Throws<Exception>(() => invoker.Execute(someCommand.Object));
            documentSessionMock.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void WhenCommandIsSentAndHandlerIsNotPresent_ExceptionThrown()
        {
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            var kernel = new StandardKernel();
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            MemoryCommandInvoker invoker = new MemoryCommandInvoker(kernel, clientSettingsMock.Object);
            Mock<ICommand> someCommand = new Mock<ICommand>();
            Assert.Throws<Ninject.ActivationException>(() => invoker.Execute(someCommand.Object));
        }

        [Test]
        public void WhenCommandIsSentAndHandlerIsNotPresent_SessionIsNotFlushed()
        {
            var kernel = new StandardKernel();

            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            MemoryCommandInvoker invoker = new MemoryCommandInvoker(kernel, clientSettingsMock.Object);
            Mock<ICommand> someCommand = new Mock<ICommand>();
            try
            {
                invoker.Execute(someCommand.Object);
            }
            catch { }

            documentSessionMock.Verify(x => x.SaveChanges(), Times.Never());
        }
         [Test]
         public void WhenCommandSentInLoop_FlushedOnlyOne()
         {

             Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
             Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
             var kernel = new StandardKernel();
             kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);
             Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
             clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                 new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
             kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
             MemoryCommandInvoker invoker = new MemoryCommandInvoker(kernel, clientSettingsMock.Object);
             Mock<ICommand> someCommand = new Mock<ICommand>();
             for (int i = 0; i < 10; i++)
             {
                 invoker.Execute(someCommand.Object);
             }
             invoker.Flush();
             documentSessionMock.Verify(x => x.SaveChanges(), Times.Once());
         }
    }
}
