using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Extensions;

namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public class InterviewerInterviewMarkerViewModel : IInterviewMarkerViewModel
{
    private readonly InterviewView interview;

    public InterviewerInterviewMarkerViewModel(InterviewView interview)
    {
        this.interview = interview;
    }

    public string Id => interview.Id;
    public MarkerType Type => MarkerType.Interview;
    public double Latitude => interview.LocationLatitude.Value;
    public double Longitude => interview.LocationLongitude.Value;
    public InterviewStatus Status => interview.Status;
    public bool CanAssign => false;
    public bool CanApproveReject => false;

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

            interviewDetails = popupTemplate;
            return interviewDetails;
        }
    }
    
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
}