using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewCreated
    {
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }

        public InterviewCreated(Guid questionnaireId, long version)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
        }
    }
}
