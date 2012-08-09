using System;
using Moq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using System.Threading;
using Questionnaire.Core.Web.Export;
using RavenQuestionnaire.Web.Utils;
using RavenQuestionnaire.Web.Controllers;


namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class ImportExportControllerTest
    {
        public Mock<IExportImport> ExportImportMock { get; set; }

        public ImportExportController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            ExportImportMock = new Mock<IExportImport>();
            Controller = new ImportExportController(ExportImportMock.Object);
        }

        [Test]
        public void When_ExportData()
        {
            var trigger = new AutoResetEvent(false);
            Controller.AsyncManager.Finished += (sender, ev) => trigger.Set();
            var clientGuid = Guid.NewGuid();
            Controller.ExportAsync(clientGuid);
            trigger.WaitOne();
            ExportImportMock.Verify(x => x.Export(clientGuid), Times.Once());
            var response = Controller.AsyncManager.Parameters["result"];
            var r = Controller.ExportCompleted(response as byte[]);
            Assert.AreEqual(r.GetType(), typeof(FileContentResult));
            Assert.AreEqual(response.GetType(), typeof(byte[]));
        }

        [Test]
        public void When_FileIsImport()
        {
            var trigger = new AutoResetEvent(false);
            var request = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var postedfile = new Mock<HttpPostedFileBase>();
            context.Setup(ctx => ctx.Request).Returns(request.Object);
            request.Setup(req => req.Files.Count).Returns(1);
            request.Setup(req => req.Files[0]).Returns(postedfile.Object);
            postedfile.Setup(f => f.ContentLength).Returns(8192).Verifiable();
            postedfile.Setup(f => f.ContentType).Returns("application/zip").Verifiable();
            postedfile.Setup(f => f.FileName).Returns("event.zip").Verifiable();
            Controller.AsyncManager.Finished += (sender, ev) => trigger.Set();
            Controller.ImportAsync(postedfile.Object);
            trigger.WaitOne();
            ExportImportMock.Verify(x => x.Import(postedfile.Object), Times.Once());
            Assert.AreEqual(request.Object.Files.Count, 1);
            Assert.AreEqual(request.Object.Files[0], postedfile.Object);
            Assert.AreEqual(request.Object.Files[0].ContentLength, 8192);
            Assert.AreEqual(request.Object.Files[0].ContentType, "application/zip");
            Assert.AreEqual(request.Object.Files[0].FileName, "event.zip");
           
        }
    }
}
