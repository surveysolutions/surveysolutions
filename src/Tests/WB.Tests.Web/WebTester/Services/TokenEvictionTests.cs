using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Enumerator.Native.WebInterview;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Web.WebTester.Services
{
    [TestFixture]
    [TestOf(typeof(TokenEviction))]
    internal class TokenEvictionTests
    {
        [Test]
        public void when_evicting_runtime_should_keep_delegated_credentials()
        {
            var interviewId = Guid.NewGuid();
            var jwtStore = new Mock<IWebTesterJwtStore>();
            var userContextStore = new Mock<IUserContextStore>();
            var eviction = CreateEviction(jwtStore.Object, userContextStore.Object);

            eviction.Evict(interviewId);

            jwtStore.Verify(x => x.Remove(interviewId), Times.Never);
            userContextStore.Verify(x => x.Remove(interviewId), Times.Never);
        }

        [Test]
        public void when_completing_interview_should_remove_delegated_credentials()
        {
            var interviewId = Guid.NewGuid();
            var jwtStore = new Mock<IWebTesterJwtStore>();
            var userContextStore = new Mock<IUserContextStore>();
            var eviction = CreateEviction(jwtStore.Object, userContextStore.Object);

            eviction.Complete(interviewId);

            jwtStore.Verify(x => x.Remove(interviewId), Times.Once);
            userContextStore.Verify(x => x.Remove(interviewId), Times.Once);
        }

        private static TokenEviction CreateEviction(
            IWebTesterJwtStore jwtStore,
            IUserContextStore userContextStore)
            => new(
                Mock.Of<IWebInterviewInvoker>(),
                Mock.Of<IAppdomainsPerInterviewManager>(),
                Mock.Of<IQuestionnaireImportService>(),
                Mock.Of<ICacheStorage<List<ICommand>, Guid>>(),
                Mock.Of<IImportStatusStore>(),
                jwtStore,
                userContextStore);
    }
}
