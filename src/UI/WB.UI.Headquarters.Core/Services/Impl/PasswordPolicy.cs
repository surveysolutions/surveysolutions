using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Configs;

namespace WB.UI.Headquarters.Services.Impl
{
    public class PasswordPolicy : IPasswordPolicy
    {
        private readonly IOptions<PasswordPolicyConfig> config;

        public PasswordPolicy(IOptions<PasswordPolicyConfig> config)
        {
            this.config = config;
        }

        /// <summary>
        /// Get minimum length required for a password
        /// </summary>
        public int PasswordMinimumLength => config.Value.PasswordMinimumLength;

        /// <summary>
        /// Gets the regular expression used to evaluate a password
        /// </summary>
        public string PasswordStrengthRegularExpression => config.Value.PasswordStrengthRegularExpression;
    }
}
