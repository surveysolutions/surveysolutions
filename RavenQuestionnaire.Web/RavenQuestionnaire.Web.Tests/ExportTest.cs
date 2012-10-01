using System;
using Main.Core.Events;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Web.Export;

namespace RavenQuestionnaire.Web.Tests
{
    //using NUnit.Framework;

    /// <summary>
    /// Summary description for ExportTest
    /// </summary>
    [TestFixture]
    public class ExportTest
    {
        [Test]
        public void When_TemplatesExport()
        {
            var synchronizer = new Mock<IEventSync>();
            var events = new TemplateExporter(synchronizer.Object);
            var result = events.ExportTemplate(Guid.NewGuid(), Guid.NewGuid());
            synchronizer.Verify(x => x.ReadEvents(), Times.Once());
            Assert.AreEqual(result.GetType(), typeof(byte[]));
        }
    }
}
