using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestOf(typeof(RosterViewModel))]
    internal class RosterViewModelTests
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
            ILiteEventRegistry eventRegistry = null, 
            IQuestionnaireStorage questionnaireRepository = null)
        {
            var viewModelFactory = new Mock<IInterviewViewModelFactory>();
            viewModelFactory.Setup(x => x.GetNew<GroupViewModel>())
                .Returns(() => new FakeGroupViewModel());

            var questionnaireRepositoryMock = new Mock<IQuestionnaireStorage>{DefaultValue = DefaultValue.Mock };
            return new RosterViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                interviewViewModelFactory ?? viewModelFactory.Object,
                eventRegistry ?? Create.Service.LiteEventRegistry(),
                Stub.MvxMainThreadAsyncDispatcher(),
                questionnaireRepository ?? questionnaireRepositoryMock.Object);
        }
    }
}
