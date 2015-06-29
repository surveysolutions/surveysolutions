using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
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
                return this.GetTeamMembersForHeadquarter();

            bool isSupervisor = viewer.Roles.Any(role => role == UserRoles.Supervisor);
            if (isSupervisor)
                return this.GetTeamMembersForSupervisor(viewer.PublicKey, input.SearchBy);

            throw new ArgumentException(String.Format("Operation is allowed only for ViewerId and Hq users. Current viewer roles are {0}",
                String.Concat(viewer.Roles)));
        }

        protected IQueryable<UserDocument> GetTeamMembersForSupervisor(Guid supervisorId, string searchBy)
        {
            List<UserDocument> userDocuments = this.users.Query(_ =>
            {
                var all = _.Where(u => !u.IsArchived);
                if (!string.IsNullOrWhiteSpace(searchBy))
                {
                    all = all.Where(x => x.UserName.Contains(searchBy) || x.Email.Contains(searchBy));
                }

                all = all.Where(user =>
                    (user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == supervisorId) ||
                        user.PublicKey == supervisorId);

                return all.ToList();
            });
            return userDocuments.AsQueryable();
        }

        protected IQueryable<UserDocument> GetTeamMembersForHeadquarter()
        {
            var result = this.users.Query(_ => _.Where(user => !user.IsArchived && user.Roles.Any(role => role == UserRoles.Operator) ||
                user.Roles.Any(role => role == UserRoles.Supervisor)).ToList()
                );
            return result.AsQueryable();
        }
    }
}