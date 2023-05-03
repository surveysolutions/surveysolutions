using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Extensions.ViewModels.Markers;

public class InterviewerAssignmentMarkerViewModel : IAssignmentMarkerViewModel
{
    private readonly AssignmentDocument assignment;

    public InterviewerAssignmentMarkerViewModel(AssignmentDocument assignment)
    {
        this.assignment = assignment;
    }

    public string Id => assignment.Id.ToString();
    public MarkerType Type => MarkerType.Assignment;
    public double Latitude => assignment.LocationLatitude.Value;
    public double Longitude => assignment.LocationLongitude.Value;
    public IDashboardItem GetDashboardItem()
    {
        throw new NotImplementedException();
    }

    public bool CanAssign => false;

    public bool CanCreateInterview
    {
        get
        {
            var interviewsByAssignmentCount = assignment.CreatedInterviewsCount ?? 0;
            var interviewsLeftByAssignmentCount = assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;
            bool canCreateInterview = !assignment.Quantity.HasValue || Math.Max(val1: 0, val2: interviewsLeftByAssignmentCount) > 0;
            return canCreateInterview;
        }
    }


    private MvxAsyncCommand createInterviewCommand;
    public MvxAsyncCommand CreateInterviewCommand => createInterviewCommand ??= new MvxAsyncCommand(CreateInterviewButtonClick);

    private bool isCreating = false;
    private async Task CreateInterviewButtonClick()
    {
        if (isCreating)
            return;
            
        isCreating = true;

        var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
        //create interview from assignment
        await viewModelNavigationService.NavigateToCreateAndLoadInterview(assignment.Id);
    }

    private string assignmentDetails;
    public string HtmlDetails
    {
        get
        {
            if (assignmentDetails != null)
                return assignmentDetails;
            
            var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
            var title = string.Format(EnumeratorUIResources.DashboardItem_Title, assignment.Title,
                questionnaireIdentity.Version);

            var interviewsByAssignmentCount = assignment.CreatedInterviewsCount ?? 0;
            var interviewsLeftByAssignmentCount = assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;

            string subTitle = "";

            if (assignment.Quantity.HasValue)
            {
                if (interviewsLeftByAssignmentCount == 1)
                {
                    subTitle = EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                }
                else
                {
                    subTitle = string.Format(EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat,
                        interviewsLeftByAssignmentCount, assignment.Quantity);
                }
            }
            else
            {
                subTitle = string.Format(EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat,
                    assignment.Quantity.GetValueOrDefault());
            }
            
            var popupTemplate = title;
            if (!string.IsNullOrWhiteSpace(subTitle))
                popupTemplate += $"\r\n{subTitle}";

            assignmentDetails = popupTemplate;
            return assignmentDetails;
        }
    }
}