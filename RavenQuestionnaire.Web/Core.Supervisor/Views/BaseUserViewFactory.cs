using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views
{
    using System.Linq.Expressions;

    public abstract class BaseUserViewFactory
    {
        protected IQueryableReadSideRepositoryReader<UserDocument> users;

        protected BaseUserViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }

        protected bool IsHq(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);
            return IsHq(viewer);
        }

        protected bool IsSupervisor(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);
            return viewer.Roles.Any(role => role == UserRoles.Supervisor);
        }

        protected void ValidateInterviewer(UserDocument user, UserDocument viewer)
        {
            if (user == null)
                return;
            if (IsHq(viewer))
                return;
            if (user.Supervisor.Id != viewer.PublicKey)
                throw new ArgumentException("informations for current user can't be displayed for this superviser");
        }

        protected IEnumerable<UserDocument> GetTeamMembersForViewer(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);

            if (viewer == null)
                return Enumerable.Empty<UserDocument>();

            if (IsHq(viewer))
                return GetTeamMembersForHeadquarter();

            bool isSupervisor = viewer.Roles.Any(role => role == UserRoles.Supervisor);
            if (isSupervisor)
                return this.GetTeamMembersForSupervisor(viewer.PublicKey);

            throw new ArgumentException(String.Format("Operation is allowed only for ViewerId and Hq users. Current viewer rolse is {0}",
                                                      String.Concat(viewer.Roles)));
        }

        protected IEnumerable<UserDocument> GetTeamMembersForSupervisor(Guid supervisorId)
        {
            return this.users.Query(_ => _
                .Where(IsSupervisorTeamMemberExpression(supervisorId))
                .ToList());
        }

        protected IEnumerable<UserDocument> GetTeamMembersForHeadquarter()
        {
            return this.users.Query(_ => _
                .Where(IsHeadquarterTeamMemberExpression())
                .ToList());
        }

        protected IEnumerable<UserDocument> GetInterviewersListForViewer(Guid viewerId)
        {
            return this
                .GetTeamMembersForViewer(viewerId)
                .Where(viewer => viewer.Roles.Any(role => role == UserRoles.Operator));
        }

        protected IEnumerable<UserDocument> GetSupervisorsListForViewer(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);

            if (viewer == null || !IsHq(viewer))
                return Enumerable.Empty<UserDocument>();

            return this.users.Query(_ => _
                .Where(IsSupervisorExpression())
                .ToList());
        }

        protected static bool IsHq(UserDocument user)
        {
            return user.Roles.Any(role => role == UserRoles.Headquarter);
        }

        private static Expression<Func<UserDocument, bool>> IsSupervisorExpression()
        {
            return user => user.Roles.Any(role => role == UserRoles.Supervisor);
        }

        private static Expression<Func<UserDocument, bool>> IsSupervisorTeamMemberExpression(Guid supervisorId)
        {
            return user =>
                (user.Roles.Any(role => role == UserRoles.Operator) && user.Supervisor.Id == supervisorId)
                || user.PublicKey == supervisorId;
        }

        private static Expression<Func<UserDocument, bool>> IsHeadquarterTeamMemberExpression()
        {
            return user =>
                user.Roles.Any(role => role == UserRoles.Operator)
                || user.Roles.Any(role => role == UserRoles.Supervisor);
        }
    }
}
