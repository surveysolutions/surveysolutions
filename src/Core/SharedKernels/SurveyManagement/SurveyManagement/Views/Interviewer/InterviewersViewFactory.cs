using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersViewFactory : IInterviewersViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public InterviewersViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }

        public InterviewersView Load(InterviewersInputModel input)
        {
            IQueryable<UserDocument> interviewers = this.GetInterviewersListForViewer(input);
            
            var items = interviewers.OrderUsingSortExpression(input.Order)
                                    .Skip((input.Page - 1) * input.PageSize)
                                    .Take(input.PageSize)
                                    .ToList()
                                    .Select(x => new InterviewersItem(x.PublicKey, x.UserName, x.Email, x.CreationDate, x.IsLockedBySupervisor, x.IsLockedByHQ, x.DeviceId));

            return new InterviewersView() {Items = items, TotalCount = interviewers.Count()};
        }

        protected IQueryable<UserDocument> GetInterviewersListForViewer(InterviewersInputModel input)
        {
            return this.GetTeamMembersForViewer(input)
                .Where(viewer => viewer.Roles.Any(role => role == UserRoles.Operator));
        }

        protected IQueryable<UserDocument> GetTeamMembersForViewer(InterviewersInputModel input)
        {
            var viewer = this.users.GetById(input.ViewerId);

            if (viewer == null)
                return Enumerable.Empty<UserDocument>().AsQueryable();

            if (viewer.IsHq())
                return this.GetTeamMembersForHeadquarter(input);

            if (viewer.IsAdmin())
                return this.GetTeamMembersForHeadquarter(input);

            bool isSupervisor = viewer.IsSupervisor();

            if (isSupervisor && input.SupervisorId.HasValue && viewer.PublicKey != input.SupervisorId.Value)
                return Enumerable.Empty<UserDocument>().AsQueryable();

            if (isSupervisor)
                return this.GetTeamMembersForSupervisor(viewer.PublicKey, input.SearchBy, input.Archived, input.ShowOnlyNotConnectedToDevice);

            throw new ArgumentException(String.Format("Operation is allowed only for ViewerId and Hq users. Current viewer roles are {0}",
                String.Concat(viewer.Roles)));
        }

        protected IQueryable<UserDocument> GetTeamMembersForSupervisor(Guid supervisorId, string searchBy, bool archived, bool showOnlyNotConnectedToDevice)
        {
            List<UserDocument> userDocuments = this.users.Query(_ =>
            {
                var all = _.Where(u => u.IsArchived == archived);
                if (!string.IsNullOrWhiteSpace(searchBy))
                {
                    var searchByToLower = searchBy.ToLower();
                    all = all.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
                }

                if (showOnlyNotConnectedToDevice)
                {
                    all = all.Where(x => x.DeviceId == null);
                }

                all = all.Where(user =>
                    (user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == supervisorId) ||
                        user.PublicKey == supervisorId);

                return all.ToList();
            });
            return userDocuments.AsQueryable();
        }

        protected IQueryable<UserDocument> GetTeamMembersForHeadquarter(InterviewersInputModel input)
        {
            var result = this.users.Query(_ => _.Where(user => user.IsArchived == input.Archived && user.Roles.Any(role => role == UserRoles.Operator) ||
                user.Roles.Any(role => role == UserRoles.Supervisor)).ToList()
                );
            return result.AsQueryable();
        }
    }
}