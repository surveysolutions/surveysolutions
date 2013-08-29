namespace WB.UI.Designer
{
    using System.ComponentModel.DataAnnotations;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Shared.Web.MembershipProvider.Accounts;

    /// <summary>
    ///     The custom string length attribute.
    /// </summary>
    public class PasswordStringLengthAttribute : StringLengthAttribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordStringLengthAttribute"/> class.
        /// </summary>
        /// <param name="maximumLength">
        /// The maximum length.
        /// </param>
        public PasswordStringLengthAttribute(int maximumLength)
            : base(maximumLength)
        {
            base.MinimumLength = this.MinimumLength;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the minimum length.
        /// </summary>
        public new int MinimumLength
        {
            get
            {
                return this.passwordPolicy.PasswordMinimumLength;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the password policy.
        /// </summary>
        private IPasswordPolicy passwordPolicy
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IPasswordPolicy>();
            }
        }

        #endregion
    }
}