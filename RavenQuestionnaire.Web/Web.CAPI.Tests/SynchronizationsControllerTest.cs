// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizationsControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   This is a test class for SynchronizationsControllerTest and is intended
//   to contain all SynchronizationsControllerTest Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

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
        public Mock<IEventSync> Synchronizer { get; set; }

        /// <summary>
        /// Gets or sets ViewRepository.
        /// </summary>
        public Mock<IViewRepository> ViewRepository { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.GlobalProvider = new Mock<IGlobalInfoProvider>();
            this.Synchronizer = new Mock<IEventSync>();
            this.ViewRepository = new Mock<IViewRepository>();
        }

        /// <summary>
        /// A test for PushStatistics
        /// </summary>
        [Test]
        public void ExportStatistics_ReturnsNotNull()
        {
            this.Synchronizer.Setup(s => s.ReadEvents()).Returns(
                new List<AggregateRootEvent> {
                        new AggregateRootEvent(
                            new CommittedEvent(
                            Guid.Empty, Guid.Empty, Guid.Empty, 1, DateTime.Now, new object(), new Version(1, 0))), 
                        new AggregateRootEvent(
                            new CommittedEvent(
                            Guid.Empty, Guid.Empty, Guid.Empty, 1, DateTime.Now, new object(), new Version(1, 0))), 
                        new AggregateRootEvent(
                            new CommittedEvent(
                            Guid.Empty, Guid.Empty, Guid.NewGuid(), 1, DateTime.Now, new object(), new Version(1, 0))), 
                    });

            this.ViewRepository.Setup(
                v =>
                v.Load<ExporStatisticsInputModel, ExportStatisticsView>(
                    new ExporStatisticsInputModel(new List<Guid> { Guid.Empty }))).Returns(
                        new ExportStatisticsView(new List<CompleteQuestionnaireBrowseItem>()));

            this.Controller = new SynchronizationsController(
                this.ViewRepository.Object, this.GlobalProvider.Object, this.Synchronizer.Object);

            JsonResult expected = null;
            JsonResult actual = this.Controller.PushStatistics();

            Assert.AreNotEqual(null, actual);
        }
        #endregion
    }
}