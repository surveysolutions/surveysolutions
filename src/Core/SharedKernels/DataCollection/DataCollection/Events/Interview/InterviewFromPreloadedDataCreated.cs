using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewFromPreloadedDataCreated : InterviewActiveEvent
    {
        public InterviewFromPreloadedDataCreated(Guid userId, Guid questionnaireId, long questionnaireVersion, int? assignmentId)
            : base(userId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.AssignmentId = assignmentId;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public int? AssignmentId { get; private set; }
    }
}
