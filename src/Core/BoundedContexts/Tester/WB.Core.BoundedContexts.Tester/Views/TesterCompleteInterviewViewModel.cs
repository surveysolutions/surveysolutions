using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Tester.Views;


public class TesterCompleteInterviewViewModel : CompleteInterviewViewModel
{
    public TesterCompleteInterviewViewModel(IViewModelNavigationService viewModelNavigationService, 
        ICommandService commandService, IPrincipal principal, 
        IEntitiesListViewModelFactory entitiesListViewModelFactory, ILastCompletionComments lastCompletionComments, 
        InterviewStateViewModel interviewState, DynamicTextViewModel dynamicTextViewModel, ILogger logger) 
        : base(viewModelNavigationService, commandService, principal, entitiesListViewModelFactory, 
            lastCompletionComments, interviewState, dynamicTextViewModel, logger)
    {
    }

    public override void Configure(string interviewUid, NavigationState navigationState)
    {
        base.Configure(interviewUid, navigationState);
        
        if (!this.InterviewState.HasCriticalFeature(interviewUid))
        {
            IsCompletionAllowed = true;
            IsLoading = false;
        }
        else
            Task.Run(() => CollectCriticalityInfo(interviewUid, navigationState));
    }
    
    protected override bool CalculateIsCompletionAllowed()
    {
        if (!this.InterviewState.HasCriticalFeature(InterviewId.FormatGuid()))
            return true;

        return !HasCriticalIssues;
    }
}
