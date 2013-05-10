// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportExportControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   The import export controller test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Questionnaire.Core.Web.Helpers;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using DataEntryClient.SycProcess;
    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessFactory;

    using Main.Core.Export;
    using Main.Core.View;

    using Moq;

    using NUnit.Framework;

    using WB.UI.Shared.Log;

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


        /// <summary>
        /// Gets or sets SyncProcessFactoryMock.
        /// </summary>
        public Mock<ISyncProcessFactory> SyncProcessFactoryMock { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
           

            this.Controller = NewImportExportController();
        }

        private ImportExportController NewImportExportController()
        {
            this.DataExportMock = new Mock<IDataExport>();
            this.SyncProcessFactoryMock = new Mock<ISyncProcessFactory>();
            var syncProcessMock = new Mock<IUsbSyncProcess>();

            this.SyncProcessFactoryMock.Setup(
                f => f.GetProcess(It.IsAny<SyncProcessType>(), It.IsAny<Guid>(), It.IsAny<Guid?>())).Returns(
                    syncProcessMock.Object);
            return new ImportExportController(
                this.DataExportMock.Object,
                (new Mock<IViewRepository>()).Object,
                this.SyncProcessFactoryMock.Object,
                (new Mock<ILog>()).Object);
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

        #endregion
    }
}