// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RavenMockUtils.cs" company="">
//   
// </copyright>
// <summary>
//   The raven mock utils.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
#if !MONODROID
namespace RavenQuestionnaire.Core.Tests.Utils
{
    using System;
    using System.Collections.Generic;

    using Moq;

    using Raven.Client;
    using Raven.Client.Linq;

    /// <summary>
    /// The raven mock utils.
    /// </summary>
    public static class RavenMockUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// The setup query result.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        /// <param name="indexName">
        /// The index name.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public static void SetupQueryResult<T>(
            this Mock<IDocumentSession> documentSession, string indexName, IEnumerable<T> result)
        {
            var ravenQueryableMock = new Mock<IRavenQueryable<T>>();
            ravenQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => result.GetEnumerator());
            ravenQueryableMock.Setup(x => x.Customize(It.IsAny<Action<object>>()).GetEnumerator()).Returns(
                () => result.GetEnumerator());

            documentSession.Setup(s => s.Query<T>(indexName, false)).Returns(ravenQueryableMock.Object).Verifiable();
        }

        /// <summary>
        /// The setup query result.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public static void SetupQueryResult<T>(this Mock<IDocumentSession> documentSession, IEnumerable<T> result)
        {
            var ravenQueryableMock = new Mock<IRavenQueryable<T>>();
            ravenQueryableMock.Setup(x => x.GetEnumerator()).Returns(result.GetEnumerator);
            ravenQueryableMock.Setup(x => x.Customize(It.IsAny<Action<object>>()).GetEnumerator()).Returns(
                () => result.GetEnumerator());
            documentSession.Setup(s => s.Query<T>()).Returns(ravenQueryableMock.Object).Verifiable();
        }

        #endregion
    }
}
#endif