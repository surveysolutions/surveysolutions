using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IPasswordValidator
    {
        Task<IdentityResult> ValidateAsync(string item);
    }


    public class HqPasswordValidator : IPasswordValidator
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
