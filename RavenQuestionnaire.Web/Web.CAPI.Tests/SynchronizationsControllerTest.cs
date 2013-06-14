// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizationsControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   This is a test class for SynchronizationsControllerTest and is intended
//   to contain all SynchronizationsControllerTest Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Core.CAPI.Views.Synchronization;

using Main.Core.View.SyncProcess;

namespace RavenQuestionnaire.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DataEntryClient.SycProcessFactory;

    using global::Core.CAPI.Views.ExporStatistics;

    using Main.Core.Events;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;

    using Moq;

    using Ncqrs.Eventing;

    using NUnit.Framework;

    using Questionnaire.Core.Web.Helpers;

    using global::Web.CAPI.Controllers;

    /// <summary>
    /// This is a test class for SynchronizationsControllerTest and is intended
    /// to contain all SynchronizationsControllerTest Unit Tests
    /// </summary>
    [TestFixture]
    public class SynchronizationsControllerTest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets Controller.
        /// </summary>
        public SynchronizationsController Controller { get; set; }

        /// <summary>
        /// Gets or sets GlobalProvider.
        /// </summary>
        public Mock<IGlobalInfoProvider> GlobalProvider { get; set; }

        /// <summary>
        /// Gets or sets Synchronizer.
        /// </summary>
        public Mock<IEventStreamReader> Synchronizer { get; set; }

        /// <summary>
        /// Gets or sets ViewRepository.
        /// </summary>
        public Mock<IViewRepository> ViewRepository { get; set; }

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
            this.GlobalProvider = new Mock<IGlobalInfoProvider>();
            this.Synchronizer = new Mock<IEventStreamReader>();
            this.ViewRepository = new Mock<IViewRepository>();
            this.SyncProcessFactoryMock = new Mock<ISyncProcessFactory>();
        }

        /// <summary>
        /// A test for PushStatistics
        /// </summary>
        [Test]
        public void ExportStatistics_ReturnsNotNull()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.Synchronizer.Setup(s => s.ReadEvents()).Returns(
                new List<AggregateRootEvent> {
                        new AggregateRootEvent(
                            new CommittedEvent(
                            Guid.Empty, Guid.Empty, guid1, 1, DateTime.Now, new object(), new Version(1, 0))), 
                        new AggregateRootEvent(
                            new CommittedEvent(
                            Guid.Empty, Guid.Empty, guid1, 1, DateTime.Now, new object(), new Version(1, 0))), 
                        new AggregateRootEvent(
                            new CommittedEvent(
                            Guid.Empty, Guid.Empty, guid2, 1, DateTime.Now, new object(), new Version(1, 0))), 
                    });

            Mock<IViewFactory<ExporStatisticsInputModel, ExportStatisticsView>> exportStatisticsViewFactoryMock =
                Mock.Get(Mock.Of<IViewFactory<ExporStatisticsInputModel, ExportStatisticsView>>(factory
                    => factory.Load(It.IsAny<ExporStatisticsInputModel>()) == new ExportStatisticsView(new List<CompleteQuestionnaireBrowseItem>())));

            this.Controller = new SynchronizationsController(
                this.GlobalProvider.Object, this.Synchronizer.Object, this.SyncProcessFactoryMock.Object,
                Mock.Of<IViewFactory<SyncProgressInputModel, SyncProgressView>>(),
                Mock.Of<IViewFactory<SyncProcessInputModel, SyncProcessView>>(),
                exportStatisticsViewFactoryMock.Object);
            this.Controller.PushStatistics();

            exportStatisticsViewFactoryMock.Verify(x => x.Load(It.Is<ExporStatisticsInputModel>(m => m.Keys.Count == 2 && m.Keys[0] == guid1 && m.Keys[1] == guid2)));
        }
        #endregion
    }
}