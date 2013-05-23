// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MembershipUser.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Shared.Web.Membership
{
    using System;
    using System.Web.Security;
    
    /// <summary>
    /// The membership user.
    /// </summary>
    public class MembershipWebUser : IMembershipWebUser
    {
        #region Fields

        private readonly IMembershipHelper hepler;

        #endregion

        #region Constructors and Destructors

        public MembershipWebUser(IMembershipHelper helper)
        {
            this.hepler = helper;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public MembershipUser MembershipUser
        {
            get
            {
                return Membership.GetUser();
            }
        }

        /// <summary>
        /// Gets the current user id.
        /// </summary>
        public Guid UserId
        {
            get
            {
                return (Guid)this.MembershipUser.ProviderUserKey;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is admin.
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return Roles.IsUserInRole(hepler.ADMINROLENAME);
            }
        }

        #endregion
    }
}