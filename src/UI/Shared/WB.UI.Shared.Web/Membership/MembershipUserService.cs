// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MembershipUserService.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Shared.Web.Membership
{
    using WebMatrix.WebData;

    /// <summary>
    ///     The user helper.
    /// </summary>
    public class MembershipUserService : IMembershipUserService
    {
        #region Fields

        /// <summary>
        /// The helper.
        /// </summary>
        private readonly IMembershipHelper helper;

        /// <summary>
        /// The web service user.
        /// </summary>
        private readonly IMembershipWebServiceUser webServiceUser;

        /// <summary>
        /// The web user.
        /// </summary>
        private readonly IMembershipWebUser user;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MembershipUserService"/> class.
        /// </summary>
        /// <param name="helper">
        /// The helper.
        /// </param>
        /// <param name="user">
        /// The web User.
        /// </param>
        /// <param name="webServiceUser">
        /// The web Service User.
        /// </param>
        public MembershipUserService(
            IMembershipHelper helper, IMembershipWebUser user, IMembershipWebServiceUser webServiceUser)
        {
            this.helper = helper;
            this.user = user;
            this.webServiceUser = webServiceUser;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the adminrolename.
        /// </summary>
        public string ADMINROLENAME
        {
            get
            {
                return this.helper.ADMINROLENAME;
            }
        }

        /// <summary>
        /// Gets the current web service user.
        /// </summary>
        public IMembershipWebServiceUser WebServiceUser
        {
            get
            {
                return this.webServiceUser;
            }
        }

        /// <summary>
        /// Gets the current web user.
        /// </summary>
        public IMembershipWebUser WebUser
        {
            get
            {
                return this.user;
            }
        }

        /// <summary>
        ///     The userrolename.
        /// </summary>
        public string USERROLENAME
        {
            get
            {
                return this.helper.USERROLENAME;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The logout.
        /// </summary>
        public void Logout()
        {
            WebSecurity.Logout();
        }

        #endregion
    }
}