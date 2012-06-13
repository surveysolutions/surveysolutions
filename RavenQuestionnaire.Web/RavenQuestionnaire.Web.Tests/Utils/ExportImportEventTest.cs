using Moq;
using System.IO;
using System.Web;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Web.Utils;
using RavenQuestionnaire.Core.ClientSettingsProvider;


namespace RavenQuestionnaire.Web.Tests.Utils
{
    [TestFixture]
    public class ExportImportEventTest
    {
        public Mock<IViewRepository> viewRepositoryMock { get; set; }

        public Mock<IMemoryCommandInvoker> invoker { get; set; }

        public Mock<IClientSettingsProvider> clientProvider { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            viewRepositoryMock = new Mock<IViewRepository>();
            invoker = new Mock<IMemoryCommandInvoker>();
            clientProvider = new Mock<IClientSettingsProvider>();
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
            var events = new ExportImportEvent(invoker.Object, viewRepositoryMock.Object, clientProvider.Object);
            var result = events.Export();
            Assert.AreEqual(result.GetType(), typeof(byte[]));
        }
    }
}
