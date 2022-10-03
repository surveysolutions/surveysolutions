using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestOf(typeof(RosterViewModel))]
    internal class RosterViewModelTests : BaseMvvmCrossTest
    {
        class FakeGroupViewModel : GroupViewModel
        {
            public override void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
            {
                base.groupIdentity = entityIdentity;
            }

            public override void Dispose()
            {
            }
        }

        protected RosterViewModel CreateViewModel(IStatefulInterviewRepository interviewRepository = null,
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IViewModelEventRegistry eventRegistry = null)
        {
            var viewModelFactory = new Mock<IInterviewViewModelFactory>();
            viewModelFactory.Setup(x => x.GetNew<GroupViewModel>())
                .Returns(() => new FakeGroupViewModel());

            return new RosterViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                interviewViewModelFactory ?? viewModelFactory.Object,
                eventRegistry ?? Create.Service.LiteEventRegistry(),
                Create.Fake.MvxMainThreadDispatcher());
        }
    }
}
