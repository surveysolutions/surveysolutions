// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataExportTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Web.Tests.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;
    using Main.Core.Export;
    using Main.Core.View;
    using Main.Core.View.Export;

    using Moq;

    using Ninject;

    using NUnit.Framework;

    using Questionnaire.Core.Web.Export;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class DataExportTests
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets Kernel.
        /// </summary>
        public IKernel Kernel { get; set; }

        /// <summary>
        /// Gets or sets Supplier.
        /// </summary>
        public Mock<IEnvironmentSupplier<CompleteQuestionnaireExportView>> Supplier { get; set; }

        /// <summary>
        /// Gets or sets the command service mock.
        /// </summary>
        public Mock<IEventStreamReader> SynchronizerMock { get; set; }

        /// <summary>
        /// Gets or sets Target.
        /// </summary>
        public DataExportServiceTest Target { get; set; }

        /// <summary>
        /// Gets or sets ViewRepositoryMock.
        /// </summary>
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The collect l evels_2 l evels_ all l evels are collected.
        /// </summary>
        [Test]
        public void CollectLEvels_2LEvelsAllNonEmpty_AllLEvelsAreCollected()
        {
            var allLevels = new Dictionary<string, byte[]>();
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            var provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);
            var items = new CompleteQuestionnaireExportItem[]
                {new CompleteQuestionnaireExportItem(new CompleteGroup("test"), new Guid[0], null)};
            var topResult = new CompleteQuestionnaireExportView(
                Guid.NewGuid(), 
                "top group", 
                items, 
                new[] { guid1, guid2 }, 
                Enumerable.Empty<Guid>(), 
                new HeaderCollection());
            var subResult = new CompleteQuestionnaireExportView(Guid.NewGuid(), "sub group", items, new Guid[0],
                                                                Enumerable.Empty<Guid>(), new HeaderCollection());
            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(
                    It.Is<CompleteQuestionnaireExportInputModel>(i => !i.PropagatableGroupPublicKey.HasValue))).Returns(
                        topResult);
            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(
                    It.Is<CompleteQuestionnaireExportInputModel>(i => i.PropagatableGroupPublicKey.HasValue))).Returns(
                        subResult);
            this.Target.ProtectedCollectLEvels(
                new CompleteQuestionnaireExportInputModel(Enumerable.Empty<Guid>(), Guid.NewGuid(), null), 
                allLevels, 
                manager);

            Assert.IsTrue(allLevels.Count == 3);
            provider.Verify(x => x.DoExportToStream(topResult), Times.Once());
            provider.Verify(x => x.DoExportToStream(subResult), Times.Exactly(2));
            this.Supplier.Verify(
                x =>
                x.BuildContent(
                    It.IsAny<CompleteQuestionnaireExportView>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<FileType>()), 
                Times.Exactly(3));
        }

        /// <summary>
        /// The collect l evels_ only o ne l evel_ one l evel file is created.
        /// </summary>
        [Test]
        public void CollectLEvels_OnlyONeLEvel_OneLEvelFileIsCreated()
        {
            var allLevels = new Dictionary<string, byte[]>();
            var provider = new Mock<IExportProvider<CompleteQuestionnaireExportView>>();
            var manager = new ExportManager<CompleteQuestionnaireExportView>(provider.Object);
            var items = new CompleteQuestionnaireExportItem[] { new CompleteQuestionnaireExportItem(new CompleteGroup("test"), new Guid[0], null) };
        
            var result = new CompleteQuestionnaireExportView(Guid.NewGuid(),"res",items,Enumerable.Empty<Guid>(),Enumerable.Empty<Guid>(),new HeaderCollection());

            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(
                    It.IsAny<CompleteQuestionnaireExportInputModel>())).Returns(result);

            this.Target.ProtectedCollectLEvels(
                new CompleteQuestionnaireExportInputModel(Enumerable.Empty<Guid>(), Guid.NewGuid(), null), 
                allLevels, 
                manager);
            Assert.IsTrue(allLevels.Count == 1);
            this.Supplier.Verify(
                x => x.BuildContent(result, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileType>()), Times.Once());
            provider.Verify(x => x.DoExportToStream(result), Times.Once());
        }

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.Kernel = new StandardKernel();
            this.ViewRepositoryMock = new Mock<IViewRepository>();
            this.Supplier = new Mock<IEnvironmentSupplier<CompleteQuestionnaireExportView>>();
            this.SynchronizerMock = new Mock<IEventStreamReader>();
            this.Kernel.Bind<IViewRepository>().ToConstant(this.ViewRepositoryMock.Object);
            this.Kernel.Bind<IEventStreamReader>().ToConstant(this.SynchronizerMock.Object);
            this.Kernel.Bind<IEnvironmentSupplier<CompleteQuestionnaireExportView>>().ToConstant(this.Supplier.Object);
            this.Target = new DataExportServiceTest(this.Kernel);
        }

        /// <summary>
        /// The export data_ invalid format_ null is returned.
        /// </summary>
        [Test]
        public void ExportData_InvalidFormat_NullIsReturned()
        {
            byte[] result = this.Target.ExportData(Guid.NewGuid(), "invalid");
            Assert.IsNull(result);
        }

        #endregion

        /// <summary>
        /// Class-helper for testing protected and private methods
        /// </summary>
        public class DataExportServiceTest : DataExport
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="DataExportServiceTest"/> class. 
            /// </summary>
            /// <param name="kernel">
            /// The kernel.
            /// </param>
            public DataExportServiceTest(IKernel kernel)
                : base(kernel)
            {
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The protected collect l evels.
            /// </summary>
            /// <param name="input">
            /// The input.
            /// </param>
            /// <param name="container">
            /// The container.
            /// </param>
            /// <param name="manager">
            /// The manager.
            /// </param>
            public void ProtectedCollectLEvels(
                CompleteQuestionnaireExportInputModel input, 
                Dictionary<string, byte[]> container, 
                ExportManager<CompleteQuestionnaireExportView> manager)
            {
                this.CollectLevels(input, container, manager, string.Empty, FileType.Csv);
            }

            #endregion
        }
    }
}