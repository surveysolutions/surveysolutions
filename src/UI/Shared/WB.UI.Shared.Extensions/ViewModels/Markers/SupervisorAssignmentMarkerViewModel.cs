using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Extensions.ViewModels.Markers;

/*public class SupervisorAssignmentMarkerViewModel : IAssignmentMarkerViewModel
{
    private readonly AssignmentDocument assignment;

    public SupervisorAssignmentMarkerViewModel(AssignmentDocument assignment)
    {
        this.assignment = assignment;
    }

    public string Id => assignment.Id.ToString();
    public MarkerType Type => MarkerType.Assignment;
    public double Latitude => assignment.LocationLatitude.Value;
    public double Longitude => assignment.LocationLongitude.Value;

    public bool CanCreateInterview => false;
    public bool CanAssign => true;
    
    private MvxAsyncCommand assignCommand;
    public MvxAsyncCommand AssignCommand => assignCommand ??= new MvxAsyncCommand(AssignAssignmentButtonClick);
    
    private async Task AssignAssignmentButtonClick()
    {
        var viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
        await viewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
            new SelectResponsibleForAssignmentArgs(assignment.Id));
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

            var responsibleName = assignment.ResponsibleName;
            if (!string.IsNullOrWhiteSpace(responsibleName))
                popupTemplate += $"\r\n{responsibleName}";

            assignmentDetails = popupTemplate;
            return assignmentDetails;
        }
    }
}*/