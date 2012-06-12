using System.Threading;
using Moq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using System.Web.Routing;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Web.Utils;
using RavenQuestionnaire.Web.Controllers;


namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class ImportExportControllerTest
    {
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public Mock<IExportImport> ExportImportMock { get; set; }

        public ImportExportController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            ExportImportMock=new Mock<IExportImport>();
            Controller = new ImportExportController(ExportImportMock.Object);
        }

        [Test]
        public void Return_ExportComplete()
        {
            var r = Controller.ExportCompleted(new byte[258]);
            Assert.AreEqual(r.GetType(), typeof(FileContentResult));
            Assert.AreNotEqual("", ((FileContentResult)r).FileContents);
            Assert.AreEqual("event.zip", ((FileContentResult)r).FileDownloadName);
            Assert.AreEqual("application/zip", ((FileContentResult)r).ContentType);
        }

        [Test]
        public void When_NewFileIsImport()
        {
            /*-----------------------------Nastya's code pay attention-------------------------------------*/
            var trigger = new AutoResetEvent(false);
            /*------------------------------------------------------------------*/
            var request = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var postedfile = new Mock<HttpPostedFileBase>();
            context.Setup(ctx => ctx.Request).Returns(request.Object);
            request.Setup(req => req.Files.Count).Returns(1);
            request.Setup(req => req.Files[0]).Returns(postedfile.Object);
            postedfile.Setup(f => f.ContentLength).Returns(8192).Verifiable();
            postedfile.Setup(f => f.ContentType).Returns("application/zip").Verifiable();
            postedfile.Setup(f => f.FileName).Returns("event.zip").Verifiable();

            /*-----------------------------Nastya's code pay attention-------------------------------------*/
            Controller.AsyncManager.Finished += (sender, ev) => trigger.Set();
            /*------------------------------------------------------------------*/

            Controller.ImportAsync(postedfile.Object);
            
            /*-----------------------------Nastya's code pay attention-------------------------------------*/
            trigger.WaitOne();
            ExportImportMock.Verify(x => x.Import(It.IsAny<HttpPostedFileBase>()));
            /*------------------------------------------------------------------*/

            Assert.AreEqual(request.Object.Files.Count, 1);
            Assert.AreEqual(request.Object.Files[0], postedfile.Object);
            Assert.AreEqual(request.Object.Files[0].ContentLength, 8192);
            Assert.AreEqual(request.Object.Files[0].ContentType, "application/zip");
            Assert.AreEqual(request.Object.Files[0].FileName, "event.zip");
           
        }
    }
}
