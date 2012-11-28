// -----------------------------------------------------------------------
// <copyright file="DataExportTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View;
using Main.Core.View.Export;
using Moq;
using NUnit.Framework;
using Ninject;
using Questionnaire.Core.Web.Export;
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
            this.Kernel=new StandardKernel();
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
            var allLevels = new Dictionary<string, byte[]>();
            Mock<IExportProvider<CompleteQuestionnaireExportView>> provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);


            CompleteQuestionnaireExportView result =
                new CompleteQuestionnaireExportView();
            this.ViewRepositoryMock.Setup(
                x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.IsAny<CompleteQuestionnaireExportInputModel>())).Returns(
                    result);

            var doResult = Target.ProtectedCollectLEvels(
                new CompleteQuestionnaireExportInputModel(Enumerable.Empty<Guid>(), Guid.NewGuid(), null), allLevels, manager);
            Assert.IsTrue(allLevels.Count == 1);

            Assert.IsTrue(doResult == "clear\r\ninsheet using \".csv\", comma\r\nsort PublicKey\r\ntempfile ind\r\nsave \"`ind'\"\r\n");
            provider.Verify(x => x.DoExportToStream(result), Times.Once());
        }
        [Test]
        public void CollectLEvels_2LEvels_AllLEvelsAreCollected()
        {
            var allLevels = new Dictionary<string, byte[]>();
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            Mock<IExportProvider<CompleteQuestionnaireExportView>> provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);


            CompleteQuestionnaireExportView topResult =
                new CompleteQuestionnaireExportView("top group",new CompleteQuestionnaireExportItem[0], new []{guid1,guid2},Enumerable.Empty<Guid>(), new Dictionary<Guid, HeaderItem>());
            CompleteQuestionnaireExportView subResult =
               new CompleteQuestionnaireExportView();
            this.ViewRepositoryMock.Setup(
                x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i=>!i.PropagatableGroupPublicKey.HasValue))).Returns(
                    topResult);
            this.ViewRepositoryMock.Setup(
               x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i => i.PropagatableGroupPublicKey.HasValue))).Returns(
                   subResult);
            var doResult = Target.ProtectedCollectLEvels(
                new CompleteQuestionnaireExportInputModel(Enumerable.Empty<Guid>(), Guid.NewGuid(), null), allLevels,
                manager);
            Assert.IsTrue(string.Format("clear\r\ninsheet using \"top group.csv\", comma\r\nsort PublicKey\r\ntempfile ind\r\nsave \"`ind'\"\r\nclear\r\ninsheet using \".csv\", comma\r\ngen PublicKey{0}=string(PublicKey)\r\ndrop PublicKey\r\ngen PublicKey=string(ForeignKey)\r\ndrop ForeignKey\r\nsort PublicKey\r\nmerge m:1 PublicKey using \"`ind'\"\r\ndrop _merge\r\nclear\r\ninsheet using \"1.csv\", comma\r\ngen PublicKey{1}=string(PublicKey)\r\ndrop PublicKey\r\ngen PublicKey=string(ForeignKey)\r\ndrop ForeignKey\r\nsort PublicKey\r\nmerge m:1 PublicKey using \"`ind'\"\r\ndrop _merge\r\n", guid1,guid2)==doResult);
            Assert.IsTrue(allLevels.Count == 3);
            provider.Verify(x => x.DoExportToStream(topResult), Times.Once());
            provider.Verify(x => x.DoExportToStream(subResult), Times.Exactly(2));
        }

        [Test]
        public void CollectLEvels_3LEvels_AllLEvelsAreCollected()
        {
            var allLevels = new Dictionary<string, byte[]>();
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            Mock<IExportProvider<CompleteQuestionnaireExportView>> provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);


            CompleteQuestionnaireExportView topResult =
                new CompleteQuestionnaireExportView("top group", new CompleteQuestionnaireExportItem[0], new[] { guid1}, Enumerable.Empty<Guid>(), new Dictionary<Guid, HeaderItem>());
            CompleteQuestionnaireExportView subResult =
               new CompleteQuestionnaireExportView("sub group", new CompleteQuestionnaireExportItem[0], new[] { guid2 }, Enumerable.Empty<Guid>(), new Dictionary<Guid, HeaderItem>());
            this.ViewRepositoryMock.Setup(
                x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i => !i.PropagatableGroupPublicKey.HasValue))).Returns(
                    topResult);
            this.ViewRepositoryMock.Setup(
               x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i => i.PropagatableGroupPublicKey==guid1))).Returns(
                   subResult);
            this.ViewRepositoryMock.Setup(
               x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.Is<CompleteQuestionnaireExportInputModel>(i => i.PropagatableGroupPublicKey == guid2))).Returns(
                   new CompleteQuestionnaireExportView());
            var doResult = Target.ProtectedCollectLEvels(
                new CompleteQuestionnaireExportInputModel(Enumerable.Empty<Guid>(), Guid.NewGuid(), null), allLevels,
                manager);
            Console.WriteLine(doResult);
            Assert.IsTrue(string.Format("clear\r\ninsheet using \"top group.csv\", comma\r\nsort PublicKey\r\ntempfile ind\r\nsave \"`ind'\"\r\nclear\r\ninsheet using \"sub group.csv\", comma\r\ngen PublicKey{0}=string(PublicKey)\r\ndrop PublicKey\r\ngen PublicKey=string(ForeignKey)\r\ndrop ForeignKey\r\nsort PublicKey\r\nmerge m:1 PublicKey using \"`ind'\"\r\ndrop _merge\r\nclear\r\ninsheet using \".csv\", comma\r\ngen PublicKey{1}=string(PublicKey)\r\ndrop PublicKey\r\ngen PublicKey{0}=string(ForeignKey)\r\ndrop ForeignKey\r\nsort PublicKey{0}\r\nmerge m:1 PublicKey{0} using \"`ind'\"\r\ndrop _merge\r\n",guid1,guid2)==doResult);
            Assert.IsTrue(allLevels.Count == 3);
            provider.Verify(x => x.DoExportToStream(topResult), Times.Once());
            provider.Verify(x => x.DoExportToStream(subResult), Times.Once());
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


            public string ProtectedCollectLEvels(CompleteQuestionnaireExportInputModel input, Dictionary<string, byte[]> container, ExportManager<CompleteQuestionnaireExportView> manager)
            {
                var result = new StringBuilder();
                CollectLevels(input, container, manager, result, "");
                return result.ToString();
            }
        }
    }

}
