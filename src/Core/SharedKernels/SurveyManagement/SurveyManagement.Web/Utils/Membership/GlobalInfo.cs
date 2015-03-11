using System;
using System.Web.Security;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership
{
    /// <summary>
    /// The global info.
    /// </summary>
    public class GlobalInfo
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        public static UserLight GetCurrentUser()
        {
            MembershipUser currentUser = System.Web.Security.Membership.GetUser();
            if (currentUser == null)
            {
                return null;
            }

            return new UserLight((Guid)currentUser.ProviderUserKey, currentUser.UserName);
        }

        public static bool IsHeadquarter
        {
            get
            {
                return Roles.IsUserInRole("Headquarter");
            }
        }

        public static bool IsSupervisor
        {
            get
            {
                return Roles.IsUserInRole("Supervisor");
            }
        }

        public static bool IsAdministrator
        {
            get
            {
                return Roles.IsUserInRole("Administrator");
            }
        }

        /// <summary>
        /// The is any user exist.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsAnyUserExist()
        {
            int count = 0;
            System.Web.Security.Membership.GetAllUsers(0, 1, out count);
            return count > 0;
        }

        #endregion
    }
}