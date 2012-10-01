using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Questionnaire.Core.Web.Tests
{
    using Moq;

    //using NUnit.Framework;
    using Main.Core.Events;
    using Questionnaire.Core.Web.Export;
    
    /// <summary>
    /// Summary description for ExportTest
    /// </summary>
    [TestClass]
    public class ExportTest
    {
        [TestMethod]
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
