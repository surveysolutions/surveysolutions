using System;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Views
{
    public class AllInterviewsInputModel : ListViewModelBase // TODO: move ListViewModelBase somewhere or better remove it
    {
        public Guid? QuestionnaireId { get; set; }

        public Guid? TeamLeadId { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }

        public long? QuestionnaireVersion { get; set; }
    }
}