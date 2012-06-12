using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
    public class CommandInvokerTests
    {
        [Test]
        public void WhenCommandIsSentAndHandlerIsPresent_HandlerIsInvoked()
        {
            //   ObjectFactory.ResetDefaults();
            Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() {PublicKey = Guid.NewGuid()}));
            //   ObjectFactory.Configure(x => x.For<ICommandHandler<string>>().Use(mockHandler.Object));
            var kernel = new StandardKernel();
            kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            CommandInvoker invoker = new CommandInvoker(kernel, clientSettingsMock.Object);

            Mock<ICommand> someCommand= new Mock<ICommand>();

            invoker.Execute(someCommand.Object);
            mockHandler.Verify(x => x.Handle(someCommand.Object), Times.Once());
        }

        
        [Test]
        public void WhenCommandIsSentAndHandlerIsPresent_SessionIsFlushed()
        {
            Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            var kernel = new StandardKernel();
            kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);
            Mock<IClientSettingsProvider> clientSettingsMock = new Mock<IClientSettingsProvider>();
            clientSettingsMock.Setup(x => x.ClientSettings).Returns(
                new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            CommandInvoker invoker = new CommandInvoker(kernel, clientSettingsMock.Object);
            Mock<ICommand> someCommand = new Mock<ICommand>();

            invoker.Execute(someCommand.Object);
            documentSessionMock.Verify(x => x.SaveChanges(), Times.Once());
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
            CommandInvoker invoker = new CommandInvoker(kernel, clientSettingsMock.Object);
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
            CommandInvoker invoker = new CommandInvoker(kernel, clientSettingsMock.Object);
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
            CommandInvoker invoker = new CommandInvoker(kernel, clientSettingsMock.Object);
            Mock<ICommand> someCommand = new Mock<ICommand>();
            try
            {
                invoker.Execute(someCommand.Object);
            }
            catch { }

            documentSessionMock.Verify(x => x.SaveChanges(), Times.Never());
        }
    }
}
