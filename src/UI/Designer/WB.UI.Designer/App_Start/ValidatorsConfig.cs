namespace WB.UI.Designer.App_Start
{
    using System.Web.Mvc;
    
    public class ValidatorsConfig
    {
        #region Public Methods and Operators
        
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