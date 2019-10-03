using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

using WB.UI.WebTester.Infrastructure;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.NotificationService
{
    public class LazyNotificationServiceTests
    {
        private WebInterviewLazyNotificationService Subj { get; set; }
        private Mock<IStatefulInterviewRepository> statefulRepoMock { get; set; }
        private TaskCompletionSource<string> tcs;

        [SetUp]
        public void Setup()
        {
            this.statefulRepoMock = new Mock<IStatefulInterviewRepository>();
            this.tcs = new TaskCompletionSource<string>();
            this.statefulRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns((IStatefulInterview)null)
                .Callback<string>(id => tcs.SetResult(id));

            this.Subj = new WebInterviewLazyNotificationService(statefulRepoMock.Object, Mock.Of<IQuestionnaireStorage>(), Mock.Of<IWebInterviewInvoker>());

            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<WebInterviewNotificationService>())
                .Returns(new WebInterviewNotificationService(this.statefulRepoMock.Object, Mock.Of<IQuestionnaireStorage>(), Mock.Of<IWebInterviewInvoker>()));


            var inScopeExecutor = Create.Service.InScopeExecutor(serviceLocatorMock.Object);

            InScopeExecutor.Init(inScopeExecutor);
        }

        [TearDown]
        public void TearDown()
        {
            InScopeExecutor.Init(null);
        }

        private string GetStatefulInterviewCallResult()
        {
            this.tcs.Task.Wait(TimeSpan.FromSeconds(15));
            return tcs.Task.Result;
        }

        [Test]
        public void RefreshEntitiesShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshEntities(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshRemovedEntitiesShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshRemovedEntities(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshEntitiesWithFilteredOptionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshEntitiesWithFilteredOptions(interviewId);

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshCascadingOptionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshCascadingOptions(interviewId, Identity.Create(Guid.Empty, RosterVector.Empty));

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshLinkedToListQuestionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshLinkedToListQuestions(interviewId, Array.Empty<Identity>());

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshLinkedToRosterQuestionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshLinkedToRosterQuestions(interviewId, Array.Empty<Identity>());

            Assert.That(GetStatefulInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }
    }
}
