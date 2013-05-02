using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.DenormalizerStorageExtensions
{
    public static class UserDocumentExtensions
    {
        public static bool IsHq(this UserDocument viewer)
        {
            if (viewer.Roles.Contains(UserRoles.Headquarter))
                return true;
            return false;
        }

        public static bool IsSupervisor(this UserDocument viewer)
        {
            if (viewer.Roles.Contains(UserRoles.Supervisor))
                return true;
            return false;
        }

        public static bool IsInterviewer(this UserDocument viewer)
        {
            if (viewer.Roles.Contains(UserRoles.Operator))
                return true;
            return false;
        }
        public static void ValidateInterviewer(this UserDocument user, UserDocument viewer)
        {
            if (user == null)
                return;
            if (viewer.IsHq())
                return;
            if (user.Supervisor.Id != viewer.PublicKey)
                throw new ArgumentException("informations for current user can't be displayed for this superviser");
        }

        public static IEnumerable<UserDocument> GetIntervieweresListForViewer(this IDenormalizerStorage<UserDocument> users, Guid viewerId)
        {
            var viewer = users.GetByGuid(viewerId);

            if (viewer == null)
                return Enumerable.Empty<UserDocument>();

            if (viewer.IsHq())
                return users.Query(u => u.IsInterviewer());
            else if (viewer.IsSupervisor())
                return
                    users.Query(u => u.IsInterviewer() && u.Supervisor.Id == viewer.PublicKey);

            throw new ArgumentException(
                string.Format("Operation is allowed only for ViewerId and Hq users. Current viewer rolse is {0}",
                              string.Concat(viewer.Roles)));
        }
    }
}
