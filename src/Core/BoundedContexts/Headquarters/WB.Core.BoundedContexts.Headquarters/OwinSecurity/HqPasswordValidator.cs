using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqPasswordValidator : IIdentityValidator<string>
    {
        private readonly IPasswordPolicy passwordPolicy;

        public HqPasswordValidator(IPasswordPolicy passwordPolicy)
        {
            this.passwordPolicy = passwordPolicy;
        }

        public Task<IdentityResult> ValidateAsync(string item)
        {
            if (string.IsNullOrEmpty(item) || item.Length < this.passwordPolicy.PasswordMinimumLength)
            {
                return Task.FromResult(IdentityResult.Failed($"Password must be at least {this.passwordPolicy.PasswordMinimumLength} characters long"));
            }

            var passwordRegexMatch = Regex.Match(item, this.passwordPolicy.PasswordStrengthRegularExpression);

            return Task.FromResult(passwordRegexMatch.Success && passwordRegexMatch.Index == 0 && passwordRegexMatch.Length == item.Length
                ? IdentityResult.Success
                : IdentityResult.Failed("Password must contain at least one number, one upper case character and one lower case character"));
        }
    }
}