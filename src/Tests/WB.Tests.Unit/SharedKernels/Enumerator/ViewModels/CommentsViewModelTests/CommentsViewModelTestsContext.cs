using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CommentsViewModelTests
{
    internal class CommentsViewModelTestsContext : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void TestSetup()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
    
        protected CommentsViewModel CreateCommentsViewModel(
            IStatefulInterview interview = null,
            IPrincipal principal = null,
            ICommandService commandService = null,
            IViewModelEventRegistry eventRegistry = null)
        {
            var interviewRepository = Create.Storage.InterviewRepository(interview ?? Create.AggregateRoot.StatefulInterview(Id.gA));

            var viewModel = new CommentsViewModel(interviewRepository, 
                principal ?? Create.Other.SupervisorPrincipal(),
                commandService ?? Create.Service.CommandService(),
                eventRegistry ?? Create.Service.LiteEventRegistry());
            return viewModel;
        }
    }
}
