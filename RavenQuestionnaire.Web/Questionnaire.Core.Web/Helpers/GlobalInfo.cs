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

        #endregion
    }
}