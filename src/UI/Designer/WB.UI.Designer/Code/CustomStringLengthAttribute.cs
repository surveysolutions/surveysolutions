// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomStringLengthAttribute.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer
{
    using System.ComponentModel.DataAnnotations;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Designer.Providers.Membership;

    /// <summary>
    ///     The custom string length attribute.
    /// </summary>
    public class CustomStringLengthAttribute : StringLengthAttribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStringLengthAttribute"/> class.
        /// </summary>
        /// <param name="maximumLength">
        /// The maximum length.
        /// </param>
        public CustomStringLengthAttribute(int maximumLength)
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