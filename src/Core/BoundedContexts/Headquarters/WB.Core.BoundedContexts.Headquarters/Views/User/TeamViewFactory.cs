using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Dapper;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;


namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class TeamViewFactory : ITeamViewFactory 
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IUserRepository userRepository;
        private readonly ISessionProvider sessionProvider;

        public TeamViewFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader, 
            IUserRepository userRepository,
            ISessionProvider sessionProvider)
        {
            this.interviewSummaryReader = interviewSummaryReader;
            this.userRepository = userRepository;
            this.sessionProvider = sessionProvider;
        }
         
        public UsersView GetAssigneeSupervisors(int pageSize, string searchBy)
        {
            var assigneeSupervisors = new UsersView()
            {
                Users = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByTeamLead(searchBy: searchBy, interviews: interviews)
                        .Take(pageSize)
                        .ToList()),

                TotalCountByQuery = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByTeamLead(searchBy: searchBy, interviews: interviews)
                        .Count())
            };
            FillUserRoles(assigneeSupervisors);
            return assigneeSupervisors;
        }     
        
        public UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy)
        {
            var assigneeSupervisorsAndDependentInterviewers = new UsersView()
            {
                Users = GetUsersFilteredByTeamLeadAndResponsible(searchBy, pageSize),
                TotalCountByQuery = GetCountUsersFilteredByTeamLeadAndResponsible(searchBy)
            };
            FillUserRoles(assigneeSupervisorsAndDependentInterviewers);
            return assigneeSupervisorsAndDependentInterviewers;
        }

        public UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId)
        {
            var asigneeInterviewersBySupervisor = new UsersView()
            {
                Users = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByResponsible(searchBy, supervisorId, interviews)
                        .Take(pageSize)
                        .ToList()),

                TotalCountByQuery = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByResponsible(searchBy, supervisorId, interviews)
                        .ToList().Count)
            };
            FillUserRoles(asigneeInterviewersBySupervisor);

            return asigneeInterviewersBySupervisor;
        }

        private void FillUserRoles(UsersView asigneeInterviewersBySupervisor)
        {
            var userIds = asigneeInterviewersBySupervisor.Users.Select(x => x.UserId).ToList();

            var allUsers = this.userRepository.Users.Where(x => userIds.Contains(x.Id)).Include(x => x.Roles).ToList();

            foreach (var user in asigneeInterviewersBySupervisor.Users)
            {
                user.IconClass = allUsers.FirstOrDefault(x => x.Id == user.UserId).Roles.FirstOrDefault().Role.ToString()
                    .ToLower();
            }
        }

        private static IQueryable<UsersViewItem> ApplyFilterByResponsible(string searchBy, Guid supervisorId, IQueryable<InterviewSummary> interviews)
        {
            interviews = interviews.Where(interview => interview.TeamLeadId == supervisorId);

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
            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                interviews = interviews.Where(x => x.TeamLeadName.ToLower().Contains(searchBy.ToLower()));
            }

            var responsiblesFromInterviews = interviews.GroupBy(x => new {x.TeamLeadId, x.TeamLeadName})
                .Where(x => x.Count() > 0)
                .Select(x => new UsersViewItem
                {
                    UserId = x.Key.TeamLeadId,
                    UserName = x.Key.TeamLeadName
                })
                .OrderBy(x => x.UserName);

            return responsiblesFromInterviews;
        }

        private List<UsersViewItem> GetUsersFilteredByTeamLeadAndResponsible(string searchBy, int pageSize)
        {
            string searchLowerText = searchBy?.ToLower();

            return sessionProvider.GetSession().Connection.Query<UsersViewItem>(@"
                SELECT u.username, u.userid 
                FROM (
                    SELECT DISTINCT coalesce(i1.teamleadname, i2.responsiblename) username, coalesce(i1.teamleadid, i2.responsibleid) userid
                      FROM readside.interviewsummaries i1
                        FULL JOIN readside.interviewsummaries AS i2 ON 1 = 2
                ) AS u
                WHERE @searchText is NULL OR LOWER(u.username) like @searchTextForLike
                ORDER BY u.username
                LIMIT @limit",
                    new
                    {
                        limit = pageSize,
                        searchText = searchLowerText,
                        searchTextForLike = $"%{searchLowerText}%",
                    }).ToList();
        }

        private int GetCountUsersFilteredByTeamLeadAndResponsible(string searchBy)
        {
            string searchLowerText = searchBy?.ToLower();

            return sessionProvider.GetSession().Connection.QuerySingle<int>(@"
                SELECT COUNT(u.username) 
                FROM (
                    SELECT DISTINCT coalesce(i1.teamleadname, i2.responsiblename) username
                      FROM readside.interviewsummaries i1
                        FULL JOIN readside.interviewsummaries AS i2 ON 1 = 2
                ) AS u
                WHERE @searchText is NULL OR LOWER(u.username) like @searchTextForLike",
                    new
                    {
                        searchText = searchLowerText,
                        searchTextForLike = $"%{searchLowerText}%",
                    });
        }
    }
}
