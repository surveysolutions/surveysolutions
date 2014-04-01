using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Nito.AsyncEx;
using Raven.Client;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public static class ApplicationManagerExtensions
    {
        public static ApplicationUser FindByUserIdAsync<T>(this UserManager<T> userManager, Guid userId) where T : ApplicationUser
        {
            string guidString = userId.FormatGuid();
            var result = AsyncContext.Run(() => userManager.Users.FirstOrDefaultAsync(x => x.Claims.Any(claim => claim.ClaimType == ClaimTypes.Sid && claim.ClaimValue == guidString)));
            return result;
        }
    }
}