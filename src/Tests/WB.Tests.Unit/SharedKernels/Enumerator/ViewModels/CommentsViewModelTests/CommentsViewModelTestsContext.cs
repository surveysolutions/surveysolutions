using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CommentsViewModelTests
{
    internal class CommentsViewModelTestsContext 
    {
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
