using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public class CustomPasswordValidator : IIdentityValidator<string>
    {
        public CustomPasswordValidator(int requiredLength, string pattern = null)
        {
            this.RequiredLength = requiredLength;
            this.Pattern = pattern;
        }

        private int RequiredLength { get; set; }

        private string Pattern { get; set; }

        public Task<IdentityResult> ValidateAsync(string item)
        {
            if (String.IsNullOrEmpty(item) || item.Length < RequiredLength)
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should be of length {0}", RequiredLength)));
            }

            if (!string.IsNullOrEmpty(Pattern) && !Regex.IsMatch(item, Pattern))
            {
                return Task.FromResult(IdentityResult.Failed("Password must contain at least one number, one upper case character and one lower case character"));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}