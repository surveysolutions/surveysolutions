using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.ChangeStatus
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
