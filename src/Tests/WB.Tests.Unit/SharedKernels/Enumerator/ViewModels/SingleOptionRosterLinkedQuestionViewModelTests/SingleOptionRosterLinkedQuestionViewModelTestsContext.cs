using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NSubstitute;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionRosterLinkedQuestionViewModelTests
{
    [NUnit.Framework.TestOf(typeof(SingleOptionRosterLinkedQuestionViewModel))]
    internal class SingleOptionRosterLinkedQuestionViewModelTestsContext: MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            base.Setup();
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
        }

        protected static SingleOptionRosterLinkedQuestionViewModel CreateViewModel(IStatefulInterviewRepository interviewRepository = null, 
            IQuestionnaireStorage questionnaireRepository = null)
        {
            return new SingleOptionRosterLinkedQuestionViewModel(Substitute.For<IPrincipal>(),
                questionnaireRepository ?? Substitute.For<IQuestionnaireStorage>(),
                interviewRepository ?? Substitute.For<IStatefulInterviewRepository>(),
                Create.Service.LiteEventRegistry(),
                Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                Substitute.For<QuestionInstructionViewModel>(),
                Substitute.For<AnsweringViewModel>(),
                Create.ViewModel.ThrottlingViewModel());
        }
    }
}
