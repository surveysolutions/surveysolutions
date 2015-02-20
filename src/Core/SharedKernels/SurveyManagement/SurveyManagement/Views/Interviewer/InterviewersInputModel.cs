using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersInputModel : ListViewModelBase
    {
        public InterviewersInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public string SearchBy { get; set; }

        public Guid ViewerId { get; set; }
    }
}