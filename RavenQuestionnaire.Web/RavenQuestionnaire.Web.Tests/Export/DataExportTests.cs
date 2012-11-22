// -----------------------------------------------------------------------
// <copyright file="DataExportTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using Main.Core.Events;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;
using RavenQuestionnaire.Web.Export;

namespace RavenQuestionnaire.Web.Tests.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class DataExportTests
    {
         /// <summary>
        /// Gets or sets the command service mock.
        /// </summary>
        public Mock<IEventSync> SynchronizerMock { get; set; }
            public IKernel Kernel { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public DataExportServiceTest Target { get; set; }
        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
        
            this.ViewRepositoryMock = new Mock<IViewRepository>();

            this.SynchronizerMock = new Mock<IEventSync>();
                this.Kernel.Bind<IViewRepository>().ToConstant(this.ViewRepositoryMock.Object);
            this.Kernel.Bind<IEventSync>().ToConstant(this.SynchronizerMock.Object);
            this.Target = new DataExportServiceTest(this.Kernel);
        }
         [Test]
        public void ExportData_InvalidFormat_NullIsReturned()
        {
            var result = Target.ExportData(Guid.NewGuid(), "invalid");
            Assert.IsNull(result);
        }
        [Test]
        public void CollectLEvels_OnlyONeLEvel_OneLEvelFileIsCreated()
        {
            var allLevels = new Dictionary<string, Stream>();
            Mock<IExportProvider<CompleteQuestionnaireExportView>> provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);


            CompleteQuestionnaireExportView result =
                new CompleteQuestionnaireExportView();
            this.ViewRepositoryMock.Setup(
                x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.IsAny<CompleteQuestionnaireExportInputModel>())).Returns(
                    result);

            Target.ProtectedCollectLEvels(Guid.NewGuid(), Enumerable.Empty<Guid>(), null, allLevels, manager);

            Assert.IsTrue(allLevels.Count == 1);
            provider.Verify(x => x.DoExportToStream(result), Times.Once());
        }
        [Test]
        public void CollectLEvels_2LEvels_AllLEvelsAreCollected()
        {
            var allLevels = new Dictionary<string, Stream>();
            Mock<IExportProvider<CompleteQuestionnaireExportView>> provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);


            CompleteQuestionnaireExportView topResult =
                new CompleteQuestionnaireExportView(new CompleteQuestionnaireExportItem[0], new []{Guid.NewGuid(),Guid.NewGuid()}, new Dictionary<Guid, string>());
            CompleteQuestionnaireExportView subResult =
               new CompleteQuestionnaireExportView();
            this.ViewRepositoryMock.Setup(
                x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i=>!i.PropagatableGroupPublicKey.HasValue))).Returns(
                    topResult);
            this.ViewRepositoryMock.Setup(
               x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i => i.PropagatableGroupPublicKey.HasValue))).Returns(
                   subResult);
            Target.ProtectedCollectLEvels(Guid.NewGuid(), Enumerable.Empty<Guid>(), null,allLevels, manager);

            Assert.IsTrue(allLevels.Count == 3);
            provider.Verify(x => x.DoExportToStream(topResult), Times.Once());
            provider.Verify(x => x.DoExportToStream(subResult), Times.Exactly(2));
        }
        /// <summary>
        /// Class-helper for testing protected and private methods
        /// </summary>
        public class DataExportServiceTest : DataExport
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TemplateExporterServiceTest"/> class.
            /// </summary>
            /// <param name="synchronizer">
            /// The synchronizer.
            /// </param>
            public DataExportServiceTest(IKernel kernel)
                : base(kernel)
            {
            }


            public void ProtectedCollectLEvels(Guid templateGuid,IEnumerable<Guid> questionnairies, Guid? level, Dictionary<string, Stream> container, ExportManager<CompleteQuestionnaireExportView> manager)
            {
                CollectLevels(templateGuid, questionnairies, level, container, manager);
            }
        }
    }

}
