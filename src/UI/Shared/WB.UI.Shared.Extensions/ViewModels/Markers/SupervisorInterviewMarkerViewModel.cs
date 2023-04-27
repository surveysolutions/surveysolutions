using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Extensions;

namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public class SupervisorInterviewMarkerViewModel : IInterviewMarkerViewModel
{
    private readonly InterviewView interview;
    private readonly string responsibleName;

    public SupervisorInterviewMarkerViewModel(InterviewView interview, string responsibleName)
    {
        this.interview = interview;
        this.responsibleName = responsibleName;
    }

    public string Id => interview.Id;
    public MarkerType Type => MarkerType.Interview;
    public double Latitude => interview.LocationLatitude.Value;
    public double Longitude => interview.LocationLongitude.Value;
    public InterviewStatus Status => interview.Status;

    private string interviewDetails;

    public string HtmlDetails
    {
        get
        {
            if (interviewDetails != null)
                return interviewDetails;

            var questionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireId);
            var title = string.Format(EnumeratorUIResources.DashboardItem_Title, interview.QuestionnaireTitle,
                questionnaireIdentity.Version);

            var popupTemplate = title;

            string status = Status.ToLocalizeString();
            if (!string.IsNullOrWhiteSpace(status))
                popupTemplate += $"\r\n{status}";

            if (!string.IsNullOrWhiteSpace(responsibleName))
                popupTemplate += $"\r\n{responsibleName}";

            interviewDetails = popupTemplate;
            return interviewDetails;
        }
    }

    public bool CanOpen => true;
    public bool CanAssign => true;
    public bool CanApproveReject => true;

    private MvxAsyncCommand openInterviewCommand;
    public MvxAsyncCommand OpenInterviewCommand => openInterviewCommand ??= new MvxAsyncCommand(OpenInterviewButtonClick);
    
    private async Task OpenInterviewButtonClick()
    {
        var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
        await viewModelNavigationService.NavigateToAsync<LoadingInterviewViewModel, LoadingViewModelArg>(
            new LoadingViewModelArg
            {
                InterviewId = interview.InterviewId
            }, true);
    }

    private MvxAsyncCommand assignCommand;
    public MvxAsyncCommand AssignCommand => assignCommand ??= new MvxAsyncCommand(AssignAssignmentButtonClick);
    
    private async Task AssignAssignmentButtonClick()
    {
        var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
        await viewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
            new SelectResponsibleForAssignmentArgs(interview.InterviewId));
    }

    public MvxAsyncCommand ApproveCommand => new MvxAsyncCommand(ApproveInterviewButtonClick);
    private async Task ApproveInterviewButtonClick()
    {
        var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
        await viewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
            new SelectResponsibleForAssignmentArgs(interview.InterviewId));
    }

    public MvxAsyncCommand RejectCommand => new MvxAsyncCommand(RejectInterviewButtonClick);
    private async Task RejectInterviewButtonClick()
    {
        var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
        await viewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
            new SelectResponsibleForAssignmentArgs(interview.InterviewId));
    }
}