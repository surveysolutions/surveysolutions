using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus
{
    public class ChangeStatusView
    {
        public Guid InterviewId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string QuestionnaireTitle { get; set; }
        public List<CommentedStatusHistroyView> StatusHistory { get; set; }
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }
        public InterviewStatus Status { get; set; }
    }

    public class CommentedStatusHistroyView
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public InterviewStatus Status { get; set; }
    }
}
