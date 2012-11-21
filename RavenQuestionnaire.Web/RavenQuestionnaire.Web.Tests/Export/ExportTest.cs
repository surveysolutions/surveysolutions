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
        public TemplateExporterServiceTest(IKernel kernel)
            : base(kernel)
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

        public void ProtectedCollectLEvels(Guid templateGuid, Guid? level, Dictionary<string, Stream> container, ExportManager manager)
        {
            CollectLevels(templateGuid, level, container, manager);
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
        public IKernel Kernel { get; set; }
        public TemplateExporterServiceTest Target { get; set; }
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
            this.Target = new TemplateExporterServiceTest(this.Kernel);
        }
        /// <summary>
        /// Export templates
        /// </summary>
        [Test]
        public void When_TemplatesExport()
        {
            var result = this.Target.ExportTemplate(Guid.NewGuid(), Guid.NewGuid());
            SynchronizerMock.Verify(x => x.ReadEvents(), Times.Once());
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
            SynchronizerMock.Setup(x => x.ReadEvents()).Returns(events);
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
            SynchronizerMock.Setup(x => x.ReadEvents()).Returns(events);
            var result = Target.PrivateGetTemplate(null, null);
            Assert.AreEqual(result.ToList().Count, 2);
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
            Mock<IExportProvider> provider = new Mock<IExportProvider>();
            var manager = new ExportManager(provider.Object);


            CompleteQuestionnaireExportView result =
                new CompleteQuestionnaireExportView();
            this.ViewRepositoryMock.Setup(
                x => x.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(It.IsAny<CompleteQuestionnaireExportInputModel>())).Returns(
                    result);

            Target.ProtectedCollectLEvels(Guid.NewGuid(), null, allLevels, manager);

            Assert.IsTrue(allLevels.Count == 1);
            provider.Verify(x => x.DoExportToStream(result), Times.Once());
        }
        [Test]
        public void CollectLEvels_2LEvels_AllLEvelsAreCollected()
        {
            var allLevels = new Dictionary<string, Stream>();
            Mock<IExportProvider> provider = new Mock<IExportProvider>();
            var manager = new ExportManager(provider.Object);


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
            Target.ProtectedCollectLEvels(Guid.NewGuid(), null,allLevels, manager);

            Assert.IsTrue(allLevels.Count == 3);
            provider.Verify(x => x.DoExportToStream(topResult), Times.Once());
            provider.Verify(x => x.DoExportToStream(subResult), Times.Exactly(2));
        }
    }
}
