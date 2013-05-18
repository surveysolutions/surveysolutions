// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MembershipHelper.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Shared.Web.Membership
{
    using System;

    using WB.UI.Designer.Providers.Roles;

    /// <summary>
    /// The membership helper.
    /// </summary>
    public class MembershipHelper : IMembershipHelper
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MembershipHelper"/> class.
        /// </summary>
        public MembershipHelper()
        {
            this.ADMINROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
            this.USERROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the adminrolename.
        /// </summary>
        public string ADMINROLENAME { get; private set; }

        /// <summary>
        /// Gets the userrolename.
        /// </summary>
        public string USERROLENAME { get; private set; }

        #endregion
    }
}