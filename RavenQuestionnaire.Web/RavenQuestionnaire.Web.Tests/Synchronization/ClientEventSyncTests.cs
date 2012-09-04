// -----------------------------------------------------------------------
// <copyright file="ClientEventSyncTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core;
using Web.CAPI.Synchronization;

namespace RavenQuestionnaire.Web.Tests.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class ClientEventSyncTests
    {
        [Test]
        public void ReadEvents_EventStoreIsEmpty_EmptyListReturned()
        {
            Mock<IViewRepository> repositoryMock=new Mock<IViewRepository>();
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 0);
        }
    }
}
