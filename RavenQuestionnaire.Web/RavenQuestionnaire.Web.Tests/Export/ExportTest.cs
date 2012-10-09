// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportTest.cs" company="WorldBank">
//   2012
// </copyright>
// <summary>
//   Class-helper for testing protected and private methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
        public TemplateExporterServiceTest(IEventSync synchronizer)
            : base(synchronizer)
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
        /// Export templates
        /// </summary>
        [Test]
        public void When_TemplatesExport()
        {
            var synchronizer = new Mock<IEventSync>();
            var events = new TemplateExporter(synchronizer.Object);
            var result = events.ExportTemplate(Guid.NewGuid(), Guid.NewGuid());
            synchronizer.Verify(x => x.ReadEvents(), Times.Once());
            Assert.AreEqual(result.GetType(), typeof(byte[]));
        }

        /// <summary>
        /// Check type of returning data
        /// </summary>
        [Test]
        public void When_ExportDataIsIEnumerable()
        {
            var synchronizer = new Mock<IEventSync>();
            var service = new TemplateExporterServiceTest(synchronizer.Object);
            var result = service.PrivateGetTemplate(null, Guid.NewGuid());
            Assert.AreEqual(result.GetType(), typeof(List<AggregateRootEvent>));
        }

        /// <summary>
        /// Check returning data without and with guid template
        /// </summary>
        public void When_ExportDataAllAndSingleTemplates()
        {
            var synchronizer = new Mock<IEventSync>();
            var fakeGuid = Guid.NewGuid();
            var events = new[]
                {
                    new AggregateRootEvent()
                        {
                            Payload = new SnapshootLoaded()
                                    {
                                        Template = new Snapshot(Guid.NewGuid(), 0, new QuestionnaireDocument() { PublicKey = fakeGuid })
                                    }
                        },
                    new AggregateRootEvent(),
                    new AggregateRootEvent(),
                    new AggregateRootEvent()
                };
            synchronizer.Setup(x => x.ReadEvents()).Returns(events);
            var service = new TemplateExporterServiceTest(synchronizer.Object);
            var result = service.PrivateGetTemplate(null, null);
            Assert.AreEqual(result.ToList().Count, 4);
            var resultGuidTemplate = service.PrivateGetTemplate(fakeGuid, null);
            Assert.AreEqual(resultGuidTemplate.Count(), 1);
        }

        /// <summary>
        /// Check if returning data is template of questionnaire
        /// </summary>
        public void Check_IfExportDataIsQuestionnaireDocument()
        {
            var synchronizer = new Mock<IEventSync>();
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
            synchronizer.Setup(x => x.ReadEvents()).Returns(events);
            var service = new TemplateExporterServiceTest(synchronizer.Object);
            var result = service.PrivateGetTemplate(null, null);
            Assert.AreEqual(result.ToList().Count, 2);
        }
    }
}
