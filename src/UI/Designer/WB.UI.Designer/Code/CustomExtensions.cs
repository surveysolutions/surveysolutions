using System;
using System.Globalization;
using System.Text;
using System.Web.Security;
using ICSharpCode.SharpZipLib.Zip;

namespace WB.UI.Designer.Extensions
{
    public static class CustomExtensions
    {
        public static void PutFileEntry(this ZipOutputStream stream, string filename, byte[] content)
        {
            var entry = new ZipEntry(filename) { IsUnicodeText = true };
            stream.PutNextEntry(entry);
            stream.Write(content, 0, content.Length);
        }

        public static void PutTextFileEntry(this ZipOutputStream stream, string filename, string text)
            => stream.PutFileEntry(filename, Encoding.UTF8.GetBytes(text ?? string.Empty));

        public static Guid AsGuid(this object source)
        {
            if (source == null)
                return Guid.Empty;
            return Guid.Parse(source.ToString());
        }

        public static int? InvertBooleableInt(this int? val, bool needValue)
        {
            return needValue && !val.ToBool() ? 1 : (int?)null;
        }

        public static bool ToBool(this int? val)
        {
            return val.HasValue && (val.Value == 1);
        }

        public static string ToErrorCode(this MembershipCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

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
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        public static string ToUIMessage(this AccountManageMessageId message)
        {
            return message == AccountManageMessageId.ChangePasswordSuccess
                       ? "Your password has been changed."
                       : message == AccountManageMessageId.SetPasswordSuccess
                             ? "Your password has been set."
                             : message == AccountManageMessageId.RemoveLoginSuccess
                                   ? "The external login was removed."
                                   : string.Empty;
        }

        public static string ToUIString(this DateTime source)
        {
            return DateTime.Compare(source.ToUniversalTime(), DateTime.MinValue) == 0
                       ? GlobalHelper.EmptyString
                       : source.ToString(CultureInfo.InvariantCulture);
        }
    }
}