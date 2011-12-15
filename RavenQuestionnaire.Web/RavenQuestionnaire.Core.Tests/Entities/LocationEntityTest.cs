using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class LocationEntityTest
    {
        [Test]
        public void ChangeEmailToValid_ChangesEmailInDocument()
        {
            LocationDocument innerDocument = new LocationDocument();
            innerDocument.Location = "some location";
            Location location = new Location(innerDocument);
            location.UpdateLocation("new location");

            Assert.AreEqual(innerDocument.Location, "new location");
        }
    }
}
