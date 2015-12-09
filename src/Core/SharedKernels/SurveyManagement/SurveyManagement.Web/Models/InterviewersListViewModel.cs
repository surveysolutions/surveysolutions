namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersListViewModel : UsersListViewModel
    {
        public bool? ShowOnlyNotConnectedToDevice { get; set; }

        public bool ShowOnlyArchived { get; set; }
    }
}