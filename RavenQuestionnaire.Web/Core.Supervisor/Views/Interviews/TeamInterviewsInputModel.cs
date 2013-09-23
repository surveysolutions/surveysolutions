using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamInterviewsInputModel: ListViewModelBase
    {
        public TeamInterviewsInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid? QuestionnaireId { get; set; }

        public long? QuestionnaireVersion { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid? InterviewId { get; set; }

        public InterviewStatus? Status { get; set; }

        public Guid ViewerId { get; set; }
    }
}
