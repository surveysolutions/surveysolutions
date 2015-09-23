using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardInformation
    {
        private readonly List<CensusQuestionnaireDashboardItemViewModel> censusQuestionnaires;
        private readonly List<InterviewDashboardItemViewModel> interviews;

        public DashboardInformation()
        {
            this.censusQuestionnaires = new List<CensusQuestionnaireDashboardItemViewModel>();
            this.interviews = new List<InterviewDashboardItemViewModel>();
        }

        public IEnumerable<IDashboardItem> CensusQuestionnaires
        {
            get { return this.censusQuestionnaires; }
        }

        public IEnumerable<IDashboardItem> NewInterviews { get { return interviews.Where(i => i.Status == DashboardInterviewStatus.New);} }
        public IEnumerable<IDashboardItem> StartedInterviews { get { return interviews.Where(i => i.Status == DashboardInterviewStatus.InProgress); } }
        public IEnumerable<IDashboardItem> CompletedInterviews { get { return interviews.Where(i => i.Status == DashboardInterviewStatus.Completed); } }
        public IEnumerable<IDashboardItem> RejectedInterviews { get { return interviews.Where(i => i.Status == DashboardInterviewStatus.Rejected); } }

        public void AddCensusQuestionnairesRange(IEnumerable<CensusQuestionnaireDashboardItemViewModel> censusQuestionnaires)
        {
            this.censusQuestionnaires.AddRange(censusQuestionnaires);
        }

        public void AddInterviewsRange(IEnumerable<InterviewDashboardItemViewModel> interviews)
        {
            this.interviews.AddRange(interviews);
        }
    }
}