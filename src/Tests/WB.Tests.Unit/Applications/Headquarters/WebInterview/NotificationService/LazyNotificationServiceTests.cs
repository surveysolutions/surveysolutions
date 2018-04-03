using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.NotificationService
{
    
    public class LazyNotificationServiceTests
    {
        private WebInterviewLazyNotificationService Subj { get; set; }
        private Mock<IStatefulInterviewRepository> statefullRepoMock { get; set; }
        private TaskCompletionSource<string> tcs;

        [SetUp]
        public void Setup()
        {
            this.statefullRepoMock = new Mock<IStatefulInterviewRepository>();
            this.tcs = new TaskCompletionSource<string>();
            this.statefullRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns((IStatefulInterview)null)
                .Callback<string>(id => tcs.SetResult(id));

            this.Subj = new WebInterviewLazyNotificationService(statefullRepoMock.Object, Mock.Of<IQuestionnaireStorage>(), Mock.Of<IWebInterviewInvoker>());
        }

        private string GetStatefullInterviewCallResult()
        {
            this.tcs.Task.Wait(TimeSpan.FromSeconds(15));
            return tcs.Task.Result;
        }

        [Test]
        public void RefreshEntitiesShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshEntities(interviewId);

            Assert.That(GetStatefullInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshRemovedEntitiesShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshRemovedEntities(interviewId);

            Assert.That(GetStatefullInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshEntitiesWithFilteredOptionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshEntitiesWithFilteredOptions(interviewId);

            Assert.That(GetStatefullInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshLinkedToListQuestionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshLinkedToListQuestions(interviewId, Array.Empty<Identity>());

            Assert.That(GetStatefullInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }

        [Test]
        public void RefreshLinkedToRosterQuestionsShouldBeCalledEventually()
        {
            var interviewId = Guid.NewGuid();

            Subj.RefreshLinkedToRosterQuestions(interviewId, Array.Empty<Identity>());

            Assert.That(GetStatefullInterviewCallResult(), Is.EqualTo(interviewId.FormatGuid()));
        }
    }
}