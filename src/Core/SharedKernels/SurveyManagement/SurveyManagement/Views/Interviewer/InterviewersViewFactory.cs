using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersViewFactory : IViewFactory<InterviewersInputModel, InterviewersView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public InterviewersViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users, IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.users = users;
            this.indexAccessor = indexAccessor;
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
            var indexName = typeof (UserDocumentsByBriefFields).Name;

            IQueryable<UserDocument> userDocuments = this.indexAccessor.Query<UserDocument>(indexName);
            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                userDocuments = userDocuments.Search(x => x.UserName, searchBy, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
                                   .Search(x => x.Email, searchBy, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards);
            }

            return userDocuments
                .Where(user => 
                    (user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == supervisorId) || 
                        user.PublicKey == supervisorId);
        }

        protected IQueryable<UserDocument> GetTeamMembersForHeadquarter()
        {
            var indexName = typeof (UserDocumentsByBriefFields).Name;

            return this.indexAccessor.Query<UserDocument>(indexName)
                .Where(user => user.Roles.Any(role => role == UserRoles.Operator) || 
                    user.Roles.Any(role => role == UserRoles.Supervisor));
        }
    }
}