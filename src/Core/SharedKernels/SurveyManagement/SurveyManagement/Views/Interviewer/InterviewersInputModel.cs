using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersInputModel : ListViewModelBase
    {
        public string SearchBy { get; set; }
        public Guid ViewerId {get; set; }
        public Guid? SupervisorId {get; set; }
        public bool Archived { get; set; }
        public bool ShowOnlyNotConnectedToDevice { get; set; }
    }
}