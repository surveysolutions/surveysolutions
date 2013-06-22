// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalInfo.cs" company="">
//   
// </copyright>
// <summary>
//   The global info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using System;
    using System.Web.Security;

    using Main.Core.Entities.SubEntities;

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
            MembershipUser currentUser = Membership.GetUser();
            if (currentUser == null)
            {
                return null;
            }

            // byte[] key = (byte[])currentUser.ProviderUserKey;
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

        /// <summary>
        /// The is any user exist.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsAnyUserExist()
        {
            int count = 0;
            Membership.GetAllUsers(0, 1, out count);
            return count > 0;
        }

        #endregion
    }
}