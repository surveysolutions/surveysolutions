using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
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
            
            var interviewerDetails = interviewers.OrderUsingSortExpression(input.Order)
                                    .Skip((input.Page - 1) * input.PageSize)
                                    .Take(input.PageSize)
                                    .Select(x => new InterviewersItem(x.PublicKey, x.UserName, x.Supervisor.Name, x.Email, x.CreationDate, x.IsLockedBySupervisor, x.IsLockedByHQ, x.DeviceId))
                                    .ToList();

            return new InterviewersView() { Items = interviewerDetails, TotalCount = interviewers.Count() };
        }

        protected IQueryable<UserDocument> GetInterviewersListForViewer(InterviewersInputModel input)
        {
            var viewer = this.users.GetById(input.ViewerId);

            if (viewer == null)
                return Enumerable.Empty<UserDocument>().AsQueryable();

            if (viewer.IsHq() || viewer.IsAdmin())
                return this.GetTeamMembersForHeadquarter(input);

            bool isSupervisor = viewer.IsSupervisor();
            if (isSupervisor && !input.SupervisorName.IsNullOrEmpty() && viewer.UserName != input.SupervisorName)
                return Enumerable.Empty<UserDocument>().AsQueryable();

            if (isSupervisor)
                return this.GetTeamMembersForSupervisor(viewer.PublicKey, input.SearchBy, input.Archived, input.ConnectedToDevice);

            throw new ArgumentException(String.Format("Operation is allowed only for ViewerId and Hq users. Current viewer roles are {0}",
                String.Concat(viewer.Roles)));
        }

        protected IQueryable<UserDocument> GetTeamMembersForSupervisor(Guid supervisorId, string searchBy, bool archived, bool? connectedToDevice)
        {
            var userDocuments = this.users.Query(_ =>
            {
                var all = _.Where(u => u.IsArchived == archived);
                if (!string.IsNullOrWhiteSpace(searchBy))
                {
                    var searchByToLower = searchBy.ToLower();
                    all = all.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
                }

                if (connectedToDevice.HasValue)
                {
                    all = all.Where(x => (x.DeviceId != null) == connectedToDevice.Value);
                }

                all = all.Where(user => (user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == supervisorId));

                return all;
            });
            return userDocuments.AsQueryable();
        }

        protected IQueryable<UserDocument> GetTeamMembersForHeadquarter(InterviewersInputModel input)
        {
            var result = this.users.Query(_ =>
            {
                var all = _.Where(user => user.IsArchived == input.Archived && user.Roles.Any(role => role == UserRoles.Operator));

                if (!string.IsNullOrWhiteSpace(input.SearchBy))
                {
                    var searchByToLower = input.SearchBy.ToLower();
                    all = all.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
                }

                if (input.ConnectedToDevice.HasValue)
                {
                    all = all.Where(user => (user.DeviceId != null) == input.ConnectedToDevice.Value);
                }

                if (!input.SupervisorName.IsNullOrEmpty())
                {
                    all = all.Where(user => user.Supervisor.Name == input.SupervisorName);
                }
                
                return all;
            });
            return result.AsQueryable();
        }
    }
}