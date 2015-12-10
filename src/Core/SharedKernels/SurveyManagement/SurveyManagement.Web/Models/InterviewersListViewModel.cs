namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersListViewModel : UsersListViewModel
    {
        public bool? ConnectedToDevice { get; set; }

        public bool Archived { get; set; }
    }
}