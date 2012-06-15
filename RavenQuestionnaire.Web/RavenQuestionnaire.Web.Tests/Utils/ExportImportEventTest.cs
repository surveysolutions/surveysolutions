using Moq;
using System;
using Ninject;
using System.IO;
using System.Web;
using Raven.Client;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using RavenQuestionnaire.Web.Utils;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Event;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.ClientSettingsProvider;


namespace RavenQuestionnaire.Web.Tests.Utils
{
    [TestFixture]
    public class ExportImportEventTest
    {
        public Mock<IViewRepository> viewRepositoryMock { get; set; }

        public Mock<IMemoryCommandInvoker>  invoker { get; set; }

        public Mock<IClientSettingsProvider> clientProvider { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            viewRepositoryMock = new Mock<IViewRepository>();
            clientProvider = new Mock<IClientSettingsProvider>();
            invoker = new Mock<IMemoryCommandInvoker>();
        }

        [Test]
        public void When_FileImport()
        {
            var request = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var postedfile = new Mock<HttpPostedFileBase>();
            context.Setup(ctx => ctx.Request).Returns(request.Object);
            request.Setup(req => req.Files[0]).Returns(postedfile.Object);
            postedfile.Setup(f => f.ContentLength).Returns(8192).Verifiable();
            postedfile.Setup(f => f.ContentType).Returns("application/zip").Verifiable();
            postedfile.Setup(f => f.FileName).Returns("event.zip").Verifiable();
            postedfile.Setup(f => f.InputStream).Returns(new MemoryStream(8192)).Verifiable();
            var events = new ExportImportEvent(invoker.Object, viewRepositoryMock.Object, clientProvider.Object);
            events.Import(postedfile.Object);
            Assert.AreEqual(request.Object.Files[0], postedfile.Object);
            Assert.AreEqual(request.Object.Files[0].ContentLength, 8192);
            Assert.AreEqual(request.Object.Files[0].ContentType, "application/zip");
            Assert.AreEqual(request.Object.Files[0].FileName, "event.zip");
        }

        [Test]
        public void When_EventsExport()
        {
            Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            var kernel = new StandardKernel();
            kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);
            clientProvider.Setup(x => x.ClientSettings).Returns(new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
            kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            MemoryCommandInvoker invokerMemory = new MemoryCommandInvoker(kernel, clientProvider.Object);
            var output = new EventBrowseView(0, 20, 0, new List<EventBrowseItem>());
            viewRepositoryMock.Setup(x => x.Load<EventBrowseInputModel, EventBrowseView>(It.Is<EventBrowseInputModel>(input => input.PublickKey==null))).Returns(output);
            var events = new ExportImportEvent(invokerMemory, viewRepositoryMock.Object, clientProvider.Object);
            var result = events.Export();
            Assert.AreEqual(result.GetType(), typeof(byte[]));
        }
    }
}
