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

    using WebMatrix.WebData;

    /// <summary>
    ///     The user helper.
    /// </summary>
    public static class UserHelper
    {
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
                return Membership.GetUser();
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
                return Roles.IsUserInRole(ADMINROLENAME);
            }
        }

        /// <summary>
        ///     The userrolename.
        /// </summary>
        public static string USERROLENAME { get; private set; }

        #endregion

        public static void Logout()
        {
            WebSecurity.Logout();
        }
    }
}