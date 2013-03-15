// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The user helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Models
{
    using System;
    using System.Web.Security;

    using WB.UI.Designer.Providers.Roles;

    /// <summary>
    ///     The user helper.
    /// </summary>
    public static class UserHelper
    {
        #region Static Fields

        /// <summary>
        /// The _ current user.
        /// </summary>
        private static MembershipUser _CurrentUser;

        /// <summary>
        /// The _ is admin.
        /// </summary>
        private static bool? _IsAdmin;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="UserHelper" /> class.
        /// </summary>
        static UserHelper()
        {
            ADMINROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
            USERROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     The adminrolename.
        /// </summary>
        public static string ADMINROLENAME { get; private set; }

        /// <summary>
        ///     Gets the current user.
        /// </summary>
        public static MembershipUser CurrentUser
        {
            get
            {
                return _CurrentUser ?? (_CurrentUser = Membership.GetUser());
            }
        }

        /// <summary>
        ///     Gets the current user id.
        /// </summary>
        public static Guid CurrentUserId
        {
            get
            {
                return (Guid)CurrentUser.ProviderUserKey;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is admin.
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                return _IsAdmin ?? (_IsAdmin = Roles.IsUserInRole(ADMINROLENAME)).Value;
            }
        }

        /// <summary>
        ///     The userrolename.
        /// </summary>
        public static string USERROLENAME { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The reset user.
        /// </summary>
        public static void ResetUser()
        {
            _CurrentUser = null;
            _IsAdmin = null;
        }

        #endregion
    }
}