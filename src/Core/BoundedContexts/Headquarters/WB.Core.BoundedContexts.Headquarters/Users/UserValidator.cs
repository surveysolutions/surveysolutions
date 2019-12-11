using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IIdentityValidator
    {
        Task<IdentityResult> ValidateAsync(HqUser item);
    }

    /// <summary>
    ///     Validates users before they are saved
    /// </summary>
    public class HqUserValidator : IIdentityValidator
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="manager"></param>
        public HqUserValidator(IUserRepository manager)
        {
            AllowOnlyAlphanumericUserNames = true;
            Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        ///     Only allow [A-Za-z0-9@_] in UserNames
        /// </summary>
        public bool AllowOnlyAlphanumericUserNames { get; set; }

        /// <summary>
        ///     If set, enforces that emails are non empty, valid, and unique
        /// </summary>
        public bool RequireUniqueEmail { get; set; }

        private IUserRepository Manager { get; }

        /// <summary>
        ///     Validates a user before saving
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> ValidateAsync(HqUser item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var errors = new List<string>();
            await ValidateUserName(item, errors);
            if (RequireUniqueEmail)
            {
                await ValidateEmailAsync(item, errors);
            }
            if (errors.Count > 0)
            {
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
        }

        private async Task ValidateUserName(HqUser user, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(String.Format(CultureInfo.CurrentCulture, "{0} cannot be null or empty.", "Name"));
            }
            else if (AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, @"^[A-Za-z0-9@_\.]+$"))
            {
                // If any characters are not letters or digits, its an illegal user name
                errors.Add(String.Format(CultureInfo.CurrentCulture, "User name {0} is invalid, can only contain letters or digits.", user.UserName));
            }
            else
            {
                var owner = await Manager.FindByNameAsync(user.UserName);
                if (owner != null && !EqualityComparer<Guid>.Default.Equals(owner.Id, user.Id))
                {
                    errors.Add(String.Format(CultureInfo.CurrentCulture, "Name {0} is already taken.", user.UserName));
                }
            }
        }

        // make sure email is not empty, valid, and unique
        private async Task ValidateEmailAsync(HqUser user, List<string> errors)
        {
            var email = await Manager.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(String.Format(CultureInfo.CurrentCulture, "{0} cannot be null or empty.", "Email"));
                return;
            }
            try
            {
                var m = new MailAddress(email);
            }
            catch (FormatException)
            {
                errors.Add(String.Format(CultureInfo.CurrentCulture, "Email '{0}' is invalid.", email));
                return;
            }
            var owner = await Manager.FindByEmailAsync(email);
            if (owner != null && !EqualityComparer<Guid>.Default.Equals(owner.Id, user.Id))
            {
                errors.Add(String.Format(CultureInfo.CurrentCulture, "Email '{0}' is already taken.", email));
            }
        }
    }
}
