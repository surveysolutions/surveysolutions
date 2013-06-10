using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;

namespace Core.Supervisor.Views
{
    using System.Linq.Expressions;

    public abstract class BaseUserViewFactory
    {
        protected IQueryableDenormalizerStorage<UserDocument> users;

        protected BaseUserViewFactory(IQueryableDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }

        protected bool IsHq(UserDocument user)
        {
            return user.Roles.Any(role => role == UserRoles.Headquarter);
        }

        protected bool IsSupervisor(UserDocument viewer)
        {
            return viewer.Roles.Any(role => role == UserRoles.Supervisor);
        }

        protected static bool IsOperator(UserDocument viewer)
        {
            return viewer.Roles.Any(role => role == UserRoles.Operator);
        }

        protected IEnumerable<UserDocument> GetTeamMembersForViewer(Guid viewerId)
        {
            var retval = Enumerable.Empty<UserDocument>();

            var viewer = this.users.GetById(viewerId);
            if (viewer != null)
            {
                if (this.IsHq(viewer))
                {
                    retval = this.GetTeamMembersForHeadquarter();
                }

                if (this.IsSupervisor(viewer))
                {
                    retval = this.GetTeamMembersForSupervisor(viewer.PublicKey);
                }
            }

            return retval;
        }

        protected IEnumerable<UserDocument> GetTeamMembersForSupervisor(Guid supervisorId)
        {
            return this.users.Query(_ => _.Where(IsSupervisorTeamMemberExpression(supervisorId)).ToList());
        }

        protected IEnumerable<UserDocument> GetTeamMembersForHeadquarter()
        {
            return this.users.Query(_ => _.Where(IsHeadquarterTeamMemberExpression()).ToList());
        }

        protected IEnumerable<UserDocument> GetInterviewersListForViewer(Guid viewerId)
        { 
            var viewer = this.users.GetById(viewerId);

            return this.IsHq(viewer)
                       ? this.GetSupervisorsListForViewer(viewerId)
                       : this.GetTeamMembersForViewer(viewerId).Where(IsOperator);
        }

        protected IEnumerable<UserDocument> GetSupervisorsListForViewer(Guid viewerId)
        {
            var retval = Enumerable.Empty<UserDocument>();

            var viewer = this.users.GetById(viewerId);
            if (viewer != null && this.IsHq(viewer))
            {
                retval = this.users.Query(_ => _.Where(x => this.IsSupervisor(x)).ToList());
            }

            return retval;
        }

        private static Expression<Func<UserDocument, bool>> IsSupervisorTeamMemberExpression(Guid supervisorId)
        {
            return user => (IsOperator(user) && user.Supervisor.Id == supervisorId) || user.PublicKey == supervisorId;
        }

        private static Expression<Func<UserDocument, bool>> IsHeadquarterTeamMemberExpression()
        {
            return user => IsOperator(user) || user.Roles.Any(role => role == UserRoles.Supervisor);
        }
    }
}
