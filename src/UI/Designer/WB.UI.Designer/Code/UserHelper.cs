// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The user helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using NinjectAdapter;
using WB.UI.Designer.Providers.Roles;
using WebMatrix.WebData;

namespace WB.UI.Designer.Code
{
    public interface IUserHelper
    {
        /// <summary>
        ///     The adminrolename.
        /// </summary>
        string ADMINROLENAME { get; }

        /// <summary>
        ///     Gets the current user.
        /// </summary>
        MembershipUser CurrentUser { get; }

        /// <summary>
        ///     Gets the current user id.
        /// </summary>
        Guid CurrentUserId { get; }

        /// <summary>
        ///     Gets a value indicating whether is admin.
        /// </summary>
        bool IsAdmin { get; }

        /// <summary>
        ///     The userrolename.
        /// </summary>
        string USERROLENAME { get; }

        void Logout();
    }

    /// <summary>
    ///     The user helper.
    /// </summary>
    public class UserHelper : IUserHelper
    {
        public static IUserHelper Instance {
            get { return (ServiceLocator.Current as NinjectServiceLocator).GetInstance<IUserHelper>(); }
        }

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="UserHelper" /> class.
        /// </summary>
        public UserHelper()
        {
            ADMINROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
            USERROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     The adminrolename.
        /// </summary>
        public string ADMINROLENAME { get; private set; }

        /// <summary>
        ///     Gets the current user.
        /// </summary>
        public MembershipUser CurrentUser
        {
            get
            {
                return Membership.GetUser();
            }
        }

        /// <summary>
        ///     Gets the current user id.
        /// </summary>
        public Guid CurrentUserId
        {
            get
            {
                return (Guid)CurrentUser.ProviderUserKey;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is admin.
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return Roles.IsUserInRole(ADMINROLENAME);
            }
        }

        /// <summary>
        ///     The userrolename.
        /// </summary>
        public string USERROLENAME { get; private set; }

        #endregion

        public void Logout()
        {
            WebSecurity.Logout();
        }
    }
}