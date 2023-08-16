using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public static class DesignerDbContextExtensions
    {
        public static DesignerIdentityUser? FindByNameOrEmail(this DbSet<DesignerIdentityUser> users, string nameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(nameOrEmail)) return null;

            var normalized = nameOrEmail.Trim().ToUpperInvariant();

            var result = users.FirstOrDefault(x =>
                x.NormalizedUserName == normalized || x.NormalizedEmail == normalized);
            return result;
        }

        public static async Task<DesignerIdentityUser?> FindByNameOrEmailAsync(this UserManager<DesignerIdentityUser> users, string? nameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(nameOrEmail)) return null;
            return  await users.FindByNameAsync(nameOrEmail) ?? await users.FindByEmailAsync(nameOrEmail);
        }
    }
}
