using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewActionLog : IView
    {
        public InterviewActionLog(Guid interviewId, List<InterviewActionExportView> actions)
        {
            this.InterviewId = interviewId;
            this.Actions = actions;
        }

        public Guid InterviewId { get; private set; }
        public List<InterviewActionExportView> Actions { get; private set; }
    }
}
