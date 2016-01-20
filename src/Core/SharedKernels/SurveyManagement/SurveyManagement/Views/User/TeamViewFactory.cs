using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class TeamViewFactory : ITeamViewFactory 
    {
        readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        readonly IQueryableReadSideRepositoryReader<UserDocument> usersReader;

        public TeamViewFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader,
            IQueryableReadSideRepositoryReader<UserDocument> usersReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
            this.usersReader = usersReader;
        }

        public UsersView GetInterviewers(int pageSize, string searchBy, Guid supervisorId)
        {
            var queryByUsers = new Func<IQueryable<UserDocument>, IQueryable<UserDocument>>((users) =>
                ApplyFilterByInterviewers(searchBy, supervisorId, users)
                    .OrderBy(user => user.UserName));

            return new UsersView()
            {
                Users = this.usersReader.Query(users =>
                    queryByUsers(users)
                        .Take(pageSize)
                        .ToList()
                        .Select(user => new UsersViewItem() {UserId = user.PublicKey, UserName = user.UserName})),

                TotalCountByQuery = this.usersReader.Query(users => queryByUsers(users).Count())
            };
        }

        public UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false)
        {
            var queryBySupervisorName = new Func<IQueryable<UserDocument>, IOrderedQueryable<UserDocument>>((users) =>
                ApplyFilterBySupervisors(searchBy: searchBy, users: users, showLocked: showLocked)
                    .OrderBy(user => user.UserName));

            return new UsersView()
            {
                Users = this.usersReader.Query(users =>
                    queryBySupervisorName(users)
                        .Take(pageSize)
                        .ToList()
                        .Select(x => new UsersViewItem() {UserId = x.PublicKey, UserName = x.UserName})),

                TotalCountByQuery = this.usersReader.Query(users => queryBySupervisorName(users).Count())
            };
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

        private static IQueryable<UserDocument> ApplyFilterBySupervisors(string searchBy, IQueryable<UserDocument> users, bool showLocked = false)
        {
            users = users.Where(user => !user.IsArchived)
                         .Where(user => showLocked || !user.IsLockedByHQ)
                         .Where(user => (user.Roles.Any(role => role == UserRoles.Supervisor)));

            users = ApplyFilterByUserName(searchBy, users);

            return users;
        }

        private static IQueryable<UserDocument> ApplyFilterByInterviewers(string searchBy, Guid supervisorId, IQueryable<UserDocument> users)
        {
            users = users.Where(user => !user.IsArchived && !user.IsLockedBySupervisor && !user.IsLockedByHQ)
                         .Where(user => (user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == supervisorId));

            users = ApplyFilterByUserName(searchBy, users);

            return users;
        }

        private static IQueryable<UserDocument> ApplyFilterByUserName(string searchBy, IQueryable<UserDocument> users)
        {
            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                users = users.Where(x => x.UserName.ToLower().Contains(searchBy.ToLower()));
            }
            return users;
        }
    }
}