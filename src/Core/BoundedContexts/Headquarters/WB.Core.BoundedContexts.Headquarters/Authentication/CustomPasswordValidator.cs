using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public class CustomPasswordValidator : IIdentityValidator<string>
    {
        public int RequiredLength { get; set; }

        public CustomPasswordValidator(int length)
        {
            RequiredLength = length;
        }

        public Task<IdentityResult> ValidateAsync(string item)
        {
            if (String.IsNullOrEmpty(item) || item.Length < RequiredLength)
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should be of length {0}", RequiredLength)));
            }

            string pattern = @"^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$";

            if (!Regex.IsMatch(item, pattern))
            {
                return Task.FromResult(IdentityResult.Failed("Password must contain at least one number, one upper case character and one lower case character"));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}