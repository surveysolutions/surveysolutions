using System.Web.Mvc;

namespace WB.UI.Shared.Web.DataAnnotations
{
    public class DataAnnotationsConfig
    {
        public static void RegisterAdapters()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(PasswordStringLengthAttribute), typeof(StringLengthAttributeAdapter));

            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(PasswordRegularExpressionAttribute), typeof(RegularExpressionAttributeAdapter));
        }
    }
}