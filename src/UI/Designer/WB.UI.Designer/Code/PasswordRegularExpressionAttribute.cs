// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordRegularExpressionAttribute.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer
{
    using System.ComponentModel.DataAnnotations;

    using Microsoft.Practices.ServiceLocation;

    using WB.UI.Designer.Providers.Membership;

    /// <summary>
    ///     The password regular expression attribute.
    /// </summary>
    public class PasswordRegularExpressionAttribute : RegularExpressionAttribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordRegularExpressionAttribute"/> class.
        /// </summary>
        public PasswordRegularExpressionAttribute()
            : base(ServiceLocator.Current.GetInstance<IPasswordPolicy>().PasswordStrengthRegularExpression)
        {
        }

        #endregion

        public override bool IsValid(object value)
        {
            var hasPattern = !string.IsNullOrEmpty(this.Pattern);
            return !hasPattern || base.IsValid(value);
        }
    }
}