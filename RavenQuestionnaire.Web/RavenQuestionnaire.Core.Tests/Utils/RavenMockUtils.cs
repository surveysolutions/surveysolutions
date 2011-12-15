using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Moq;
using Raven.Client;
using Raven.Client.Linq;

namespace RavenQuestionnaire.Core.Tests.Utils
{
    public static class RavenMockUtils
    {
        public static void SetupQueryResult<T>(this Mock<IDocumentSession> documentSession, string indexName, IEnumerable<T> result)
        {
            var ravenQueryableMock = new Mock<IRavenQueryable<T>>();
            ravenQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => result.GetEnumerator());
            ravenQueryableMock.Setup(x => x.Customize(It.IsAny<Action<Object>>()).GetEnumerator()).Returns(
                () => result.GetEnumerator());

            documentSession.Setup(s => s.Query<T>(indexName)).Returns(ravenQueryableMock.Object).Verifiable();
        }
        public static void SetupQueryResult<T>(this Mock<IDocumentSession> documentSession, IEnumerable<T> result)
        {
            var ravenQueryableMock = new Mock<IRavenQueryable<T>>();
            ravenQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => result.GetEnumerator());
            ravenQueryableMock.Setup(x => x.Customize(It.IsAny<Action<Object>>()).GetEnumerator()).Returns(
                () => result.GetEnumerator());
            documentSession.Setup(s => s.Query<T>()).Returns(ravenQueryableMock.Object).Verifiable();
        }
    }
}
