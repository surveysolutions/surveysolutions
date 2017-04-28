using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class TeamViewFactory : ITeamViewFactory 
    {
        readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public TeamViewFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy)
        {
            return new UsersView()
            {
                Users = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByTeamLead(searchBy: searchBy, interviews: interviews)
                        .Take(pageSize)
                        .ToList()),

                TotalCountByQuery = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByTeamLead(searchBy: searchBy, interviews: interviews)
                    .ToList()
                    .Count())
            };
        }

        public UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId)
        {
            return new UsersView()
            {
                Users = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByResponsible(searchBy, supervisorId, interviews)
                        .Take(pageSize)
                        .ToList()),

                TotalCountByQuery = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByResponsible(searchBy, supervisorId, interviews)
                        .ToList()
                        .Count())
            };
        }

        private static IQueryable<UsersViewItem> ApplyFilterByResponsible(string searchBy, Guid supervisorId, IQueryable<InterviewSummary> interviews)
        {
            interviews = interviews.Where(interview => !interview.IsDeleted && interview.TeamLeadId == supervisorId);

            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                interviews = interviews.Where(x => x.ResponsibleName.ToLower().Contains(searchBy.ToLower()));
            }

            var responsiblesFromInterviews = interviews.GroupBy(x => new { x.ResponsibleId, x.ResponsibleName })
                                                       .Where(x => x.Count() > 0)
                                                       .Select(x => new UsersViewItem { UserId = x.Key.ResponsibleId, UserName = x.Key.ResponsibleName })
                                                       .OrderBy(x => x.UserName);

            return responsiblesFromInterviews;
        }

        private static IQueryable<UsersViewItem> ApplyFilterByTeamLead(string searchBy, IQueryable<InterviewSummary> interviews)
        {
            interviews = interviews.Where(interview => !interview.IsDeleted);
            
            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                interviews = interviews.Where(x => x.TeamLeadName.ToLower().Contains(searchBy.ToLower()));
            }

            var responsiblesFromInterviews = interviews.GroupBy(x => new {x.TeamLeadId, x.TeamLeadName})
                                                       .Where(x => x.Count() > 0)
                                                       .Select(x => new UsersViewItem {UserId = x.Key.TeamLeadId, UserName = x.Key.TeamLeadName})
                                                       .OrderBy(x => x.UserName);

            return responsiblesFromInterviews;
        }
    }
}