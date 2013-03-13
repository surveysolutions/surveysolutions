using System;
using System.Web.Security;

namespace WB.UI.Designer.Extensions
{
    public static class CustomExtensions
    {
        public static string ToErrorCode(this MembershipCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        public static string ToUIMessage(this AccountManageMessageId message)
        {
            return
                message == AccountManageMessageId.ChangePasswordSuccess
                    ? "Your password has been changed."
                    : message == AccountManageMessageId.SetPasswordSuccess
                          ? "Your password has been set."
                          : message == AccountManageMessageId.RemoveLoginSuccess
                                ? "The external login was removed."
                                : "";
        }

        public static string ToUIString(this DateTime source)
        {
            return source == DateTime.MinValue ? string.Empty : source.ToString();
        }

        public static bool? InvertSpecial(this bool? val, bool needValue)
        {
            if (needValue)
            {
                val = val ?? false;
            }

            return val.HasValue ? !(bool?)val.Value : null;
        }
    }
}