using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoryInputModel
    {
        public InterviewHistoryInputModel(Guid interviewId)
        {
            this.InterviewId = interviewId;
        }

        public Guid InterviewId { get; set; }
    }
}
