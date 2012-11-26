// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportTest.cs" company="WorldBank">
//   2012
// </copyright>
// <summary>
//   Class-helper for testing protected and private methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;
using Main.Core.View;
using Ninject;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;

namespace RavenQuestionnaire.Web.Tests.Export
{
    #region Libraries

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Moq;

    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Restoring.EventStapshoot;

    using NUnit.Framework;

    using RavenQuestionnaire.Web.Export;

    using Assert = NUnit.Framework.Assert;

    #endregion

    #region Settings

    /// <summary>
    /// Class-helper for testing protected and private methods
    /// </summary>
    public class TemplateExporterServiceTest : TemplateExporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExporterServiceTest"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public TemplateExporterServiceTest(IEventSync sync)
            : base(sync)
        {
        }

        /// <summary>
        /// Method-helper for testing protected methods GetTemplate
        /// </summary>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <returns>
        /// results protected methods GetTemplate
        /// </returns>
        public IEnumerable<AggregateRootEvent> PrivateGetTemplate(Guid? templateGuid, Guid? clientGuid)
        {
            return base.GetTemplate(templateGuid, clientGuid);
        }
    }

    #endregion
    
    /// <summary>
    /// Tests for export templates
    /// </summary>
    [TestFixture]
    public class ExportTest
    {
        /// <summary>
        /// Gets or sets the command service mock.
        /// </summary>
        public Mock<IEventSync> SynchronizerMock { get; set; }

        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public TemplateExporterServiceTest Target { get; set; }
        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
        
            this.ViewRepositoryMock = new Mock<IViewRepository>();

            this.SynchronizerMock = new Mock<IEventSync>();
        
            this.Target = new TemplateExporterServiceTest(this.SynchronizerMock.Object);
        }
        /// <summary>
        /// Export templates
        /// </summary>
        [Test]
        public void When_TemplatesExport()
        {
            var result = this.Target.ExportTemplate(Guid.NewGuid(), Guid.NewGuid());
            SynchronizerMock.Verify(x => x.ReadEvents(), Times.Once());
            synchronizer.Verify(x => x.ReadEvents(null), Times.Once());
            Assert.AreEqual(result.GetType(), typeof(byte[]));
        }

        /// <summary>
        /// Check type of returning data
        /// </summary>
        [Test]
        public void When_ExportDataIsIEnumerable()
        {
            var result = this.Target.PrivateGetTemplate(null, Guid.NewGuid());
            Assert.AreEqual(result.GetType(), typeof(List<AggregateRootEvent>));
        }

        /// <summary>
        /// Check returning data without and with guid template
        /// </summary>
        [Test]
        public void When_ExportDataAllAndSingleTemplates()
        {
            var fakeGuid = Guid.NewGuid();
            var events = new[]
                {
                    new AggregateRootEvent()
                        {
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(fakeGuid, 0, new QuestionnaireDocument() { PublicKey = fakeGuid })
                                    }
                        },
                    new AggregateRootEvent(){
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(Guid.NewGuid(), 0, new QuestionnaireDocument() { PublicKey = Guid.NewGuid() })
                                    }
                        },
                    new AggregateRootEvent(){
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(Guid.NewGuid(), 0, new QuestionnaireDocument() { PublicKey = Guid.NewGuid() })
                                    }
                        },
                    new AggregateRootEvent(){
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(Guid.NewGuid(), 0, new QuestionnaireDocument() { PublicKey = Guid.NewGuid() })
                                    }
                        }
                };
            synchronizer.Setup(x => x.ReadEvents(null)).Returns(events);
            var result = Target.PrivateGetTemplate(null, null);
            Assert.AreEqual(result.ToList().Count, 4);
            var resultGuidTemplate = Target.PrivateGetTemplate(fakeGuid, null);
            Assert.AreEqual(resultGuidTemplate.Count(), 1);
        }

        /// <summary>
        /// Check if returning data is template of questionnaire
        /// </summary>
        [Test]
        public void Check_IfExportDataIsQuestionnaireDocument()
        {
            var events = new[]
                {
                    new AggregateRootEvent()
                        {
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(Guid.NewGuid(), 0, new QuestionnaireDocument())
                                    }
                        },
                    new AggregateRootEvent()
                        {
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(Guid.NewGuid(), 0, new UserDocument())
                                    }
                        },
                    new AggregateRootEvent()
                        {
                            Payload = new SnapshootLoaded()
                                {
                                    Template = new Snapshot(Guid.NewGuid(), 0, new QuestionnaireDocument())
                                }
                        },
                    new AggregateRootEvent()
                        {
                            Payload = new SnapshootLoaded()
                                {
                                    Template = new Snapshot(Guid.NewGuid(), 0, new CollectionDocument())
                                }
                        }
                };
            synchronizer.Setup(x => x.ReadEvents(null)).Returns(events);
            var result = Target.PrivateGetTemplate(null, null);
            Assert.AreEqual(result.ToList().Count, 2);
        }
       
    }
}
