namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersListViewModel : UsersListViewModel
    {
        public bool ShowOnlyNotConnectedToDevice { get; set; }
    }
}