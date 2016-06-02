using System;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class HeadquartersTeamsAndStatusesReport : AbstractTeamsAndStatusesReport, IHeadquartersTeamsAndStatusesReport
    {
        public HeadquartersTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader) : base(interviewsReader)
        {
        }

        protected override Expression<Func<InterviewSummary, object>> ResponsibleIdSelector
        {
            get { return (interivew) => interivew.TeamLeadId; }
        }

        protected override Expression<Func<InterviewSummary, object>> ResponsibleNameSelector
        {
            get { return (interivew) => interivew.TeamLeadName; }
        }
    }
}