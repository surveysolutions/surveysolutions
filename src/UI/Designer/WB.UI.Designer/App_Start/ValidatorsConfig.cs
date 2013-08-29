namespace WB.UI.Designer
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    /// <summary>
    /// The validators config.
    /// </summary>
    public class ValidatorsConfig
    {
        #region Public Methods and Operators

        /// <summary>
        /// The register.
        /// </summary>
        public static void Register()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(PasswordStringLengthAttribute), typeof(StringLengthAttributeAdapter));

            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(PasswordRegularExpressionAttribute), typeof(RegularExpressionAttributeAdapter));
        }

        #endregion
    }
}