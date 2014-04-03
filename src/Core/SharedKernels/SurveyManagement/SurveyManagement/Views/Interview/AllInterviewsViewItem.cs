using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AllInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanDelete { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { set; get; }
    }
}