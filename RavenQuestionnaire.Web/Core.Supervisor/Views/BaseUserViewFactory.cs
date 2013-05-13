using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;

namespace Core.Supervisor.Views
{
    public abstract class BaseUserViewFactory
    {
        protected IQueryableDenormalizerStorage<UserDocument> users;

        protected BaseUserViewFactory(IQueryableDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }

        protected bool IsHq(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);
            return IsHq(viewer);
        }

        protected bool IsHq(UserDocument viewer)
        {
            if (viewer.Roles.Contains(UserRoles.Headquarter))
                return true;
            return false;
        }

        protected bool IsSupervisor(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);
            return IsSupervisor(viewer);
        }

        protected bool IsSupervisor(UserDocument viewer)
        {
            if (viewer.Roles.Contains(UserRoles.Supervisor))
                return true;
            return false;
        }

        protected bool IsInterviewer(UserDocument viewer)
        {
            if (viewer.Roles.Contains(UserRoles.Operator))
                return true;
            return false;
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
            if (IsSupervisor(viewer))
                return GetTeamMembersForSupervisor(viewer);

            throw new ArgumentException(String.Format("Operation is allowed only for ViewerId and Hq users. Current viewer rolse is {0}",
                                                      String.Concat(viewer.Roles)));
        }

        protected IEnumerable<UserDocument> GetTeamMembersForSupervisor(UserDocument viewer)
        {
            return users.Query().Where(u => (IsInterviewer(u) && u.Supervisor.Id == viewer.PublicKey) || u.PublicKey == viewer.PublicKey);
        }

        protected IEnumerable<UserDocument> GetTeamMembersForHeadquarter()
        {
            return users.Query().Where(u => IsInterviewer(u) || IsSupervisor(u));
        }

        protected IEnumerable<UserDocument> GetInterviewersListForViewer(Guid viewerId)
        {
            return GetTeamMembersForViewer(viewerId).Where(IsInterviewer);
        }

        protected IEnumerable<UserDocument> GetSupervisorsListForViewer(Guid viewerId)
        {
            var viewer = users.GetById(viewerId);

            if (viewer == null || !IsHq(viewer))
                return Enumerable.Empty<UserDocument>();

            return users.Query().Where(u => IsSupervisor(u));
        }
    }
}
