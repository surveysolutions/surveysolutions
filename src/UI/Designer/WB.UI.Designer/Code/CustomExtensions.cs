// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   The custom extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Extensions
{
    using System;
    using System.Globalization;
    using System.Web.Security;

    /// <summary>
    ///     The custom extensions.
    /// </summary>
    public static class CustomExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The as guid.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public static Guid AsGuid(this object source)
        {
            return (Guid)source;
        }

        /// <summary>
        /// The invert special.
        /// </summary>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="needValue">
        /// The need value.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>int?</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static int? InvertBooleableInt(this int? val, bool needValue)
        {
            return needValue && !val.ToBool() ? 1 : (int?)null;
        }

        /// <summary>
        /// The to bool.
        /// </summary>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ToBool(this int? val)
        {
            return val.HasValue && (val.Value == 1);
        }

        /// <summary>
        /// The to error code.
        /// </summary>
        /// <param name="createStatus">
        /// The create status.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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

        /// <summary>
        /// The to ui message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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

        /// <summary>
        /// The to ui string.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToUIString(this DateTime source)
        {
            return source == DateTime.MinValue ? GlobalHelper.EmptyString : source.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}