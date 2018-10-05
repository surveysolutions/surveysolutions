using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Security;
using ICSharpCode.SharpZipLib.Zip;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Extensions
{
    public static class CustomExtensions
    {
        public static byte[] ReadToEnd(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

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
                    return AccountResources.UserNameExists;

                case MembershipCreateStatus.DuplicateEmail:
                    return AccountResources.UserNameForEmailExists;

                case MembershipCreateStatus.InvalidPassword:
                    return AccountResources.InvalidPassword;

                case MembershipCreateStatus.InvalidEmail:
                    return AccountResources.InvalidEmail;

                case MembershipCreateStatus.InvalidAnswer:
                    return AccountResources.InvalidRetrievalAnswer;

                case MembershipCreateStatus.InvalidQuestion:
                    return AccountResources.InvalidRetrievalQuestion;

                case MembershipCreateStatus.InvalidUserName:
                    return AccountResources.InvalidUserName;

                case MembershipCreateStatus.ProviderError:
                    return AccountResources.MembershipProviderError;

                case MembershipCreateStatus.UserRejected:
                    return AccountResources.UserCreationRequestCanceled;

                default:
                    return AccountResources.UnknownError;
            }
        }

        public static string ToUIMessage(this AccountManageMessageId message)
            => message == AccountManageMessageId.ChangePasswordSuccess
                ? AccountResources.PasswordChanged
                : message == AccountManageMessageId.SetPasswordSuccess
                    ? AccountResources.PasswordSet
                    : message == AccountManageMessageId.RemoveLoginSuccess
                        ? AccountResources.ExternalLoginRemoved
                        : message == AccountManageMessageId.UpdateUserProfileSuccess
                            ? AccountResources.AccountInfoUpdated
                            : string.Empty;

        public static string ToUIString(this DateTime source)
        {
            return DateTime.Compare(source.ToUniversalTime(), DateTime.MinValue) == 0
                       ? GlobalHelper.EmptyString
                       : source.ToString(CultureInfo.InvariantCulture);
        }
    }
}
