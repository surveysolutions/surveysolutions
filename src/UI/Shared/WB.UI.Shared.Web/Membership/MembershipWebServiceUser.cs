// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MembershipUser.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Shared.Web.Membership
{
    using System;
    using System.ServiceModel;
    using System.Web.Security;

    /// <summary>
    /// The membership user.
    /// </summary>
    public class MembershipWebServiceUser : IMembershipWebServiceUser
    {
        #region Fields

        private readonly IMembershipHelper hepler;

        private  string userName
        {
            get
            {
                return ServiceSecurityContext.Current.PrimaryIdentity.Name;
            }
        }
            

        #endregion

        #region Constructors and Destructors

        public MembershipWebServiceUser(IMembershipHelper helper)
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
                return Membership.GetUser(userName);
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
                return Roles.IsUserInRole(userName, hepler.ADMINROLENAME);
            }
        }

        #endregion
    }
}