using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.Tests.Web.Headquarters.Controllers.WebInterview.NotificationService
{
    public class LazyNotificationServiceTests
    {
        private WebInterviewLazyNotificationService Subj { get; set; }
        private Mock<IStatefulInterviewRepository> statefulRepoMock { get; set; }
        private TaskCompletionSource<string> tcs;
        private IAggregateRootCache aggregateRootCache;

        [SetUp]
        public void Setup()
        {
            this.statefulRepoMock = new Mock<IStatefulInterviewRepository>();
            this.tcs = new TaskCompletionSource<string>();
            this.statefulRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns((IStatefulInterview)null)
                .Callback<string>(id => tcs.SetResult(id));

            this.aggregateRootCache = Abc.Create.Storage.NewAggregateRootCache();
            
            this.Subj = new WebInterviewLazyNotificationService(statefulRepoMock.Object, 
                Mock.Of<IQuestionnaireStorage>(), Mock.Of<IWebInterviewInvoker>(), aggregateRootCache);

            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<WebInterviewNotificationService>())
                .Returns(new WebInterviewNotificationService(this.statefulRepoMock.Object, Mock.Of<IQuestionnaireStorage>(), Mock.Of<IWebInterviewInvoker>()));

            var inScopeExecutor = Abc.Create.Service.InScopeExecutor(serviceLocatorMock.Object);

            InScopeExecutor.Init(inScopeExecutor);
        }

        [TearDown]
        public void TearDown()
        {
            InScopeExecutor.Init(null);
        }

        private string GetStatefulInterviewCallResult()
        {
            this.tcs.Task.Wait(TimeSpan.FromSeconds(4));
            if (tcs.Task.IsCompleted)
            {
                return tcs.Task.Result;
            }

            return null;
        }

        [Test]
        public void RefreshEntitiesShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 1);
            Subj.RefreshEntities(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshRemovedEntitiesShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 1);
            Subj.RefreshRemovedEntities(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshEntitiesWithFilteredOptionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 1);
            Subj.RefreshEntitiesWithFilteredOptions(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshCascadingOptionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 1);
            Subj.RefreshCascadingOptions(interviewId, Identity.Create(Guid.Empty, RosterVector.Empty));

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshLinkedToListQuestionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 1);
            Subj.RefreshLinkedToListQuestions(interviewId, Array.Empty<Identity>());

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshLinkedToRosterQuestionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 1);
            Subj.RefreshLinkedToRosterQuestions(interviewId, Array.Empty<Identity>());

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void NoRefreshEntiesCallsIfNoOneConnected()
        {
            var interviewId = Guid.NewGuid();
            aggregateRootCache.SetConnectedCount(interviewId, 0);
            Subj.RefreshEntities(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(null));
        }
    }
}
