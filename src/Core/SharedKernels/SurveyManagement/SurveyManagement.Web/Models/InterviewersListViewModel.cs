namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersListViewModel : UsersListViewModel
    {
        public string SupervisorName { get; set; }

        public bool? ConnectedToDevice { get; set; }

        public bool Archived { get; set; }
    }
}