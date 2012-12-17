// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   The import export controller test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    using System;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Export;

    using Moq;

    using NUnit.Framework;

    using global::Web.Supervisor.Controllers;

    /// <summary>
    /// The import export controller test.
    /// </summary>
    [TestFixture]
    public class ImportExportControllerTest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets Controller.
        /// </summary>
        public ImportExportController Controller { get; set; }

        /// <summary>
        /// Gets or sets DataExportMock.
        /// </summary>
        public Mock<IDataExport> DataExportMock { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.DataExportMock = new Mock<IDataExport>();
            this.Controller = new ImportExportController(this.DataExportMock.Object);
        }

        /// <summary>
        /// The when_ export data.
        /// </summary>
        [Test]
        public void When_ExportData()
        {
            var trigger = new AutoResetEvent(false);
            this.Controller.AsyncManager.Finished += (sender, ev) => trigger.Set();
            Guid clientGuid = Guid.NewGuid();
            this.Controller.ExportAsync(clientGuid);
            trigger.WaitOne();
            object response = this.Controller.AsyncManager.Parameters["result"];
            ActionResult r = this.Controller.ExportCompleted(response as byte[]);
            Assert.AreEqual(r.GetType(), typeof(FileContentResult));
            Assert.AreEqual(response.GetType(), typeof(byte[]));
        }

        /// <summary>
        /// The when_ file is import.
        /// </summary>
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
            this.Controller.AsyncManager.Finished += (sender, ev) => trigger.Set();
            this.Controller.Import(postedfile.Object);
            trigger.WaitOne();
            Assert.AreEqual(request.Object.Files.Count, 1);
            Assert.AreEqual(request.Object.Files[0], postedfile.Object);
            Assert.AreEqual(request.Object.Files[0].ContentLength, 8192);
            Assert.AreEqual(request.Object.Files[0].ContentType, "application/zip");
            Assert.AreEqual(request.Object.Files[0].FileName, "event.zip");
        }

        #endregion
    }
}