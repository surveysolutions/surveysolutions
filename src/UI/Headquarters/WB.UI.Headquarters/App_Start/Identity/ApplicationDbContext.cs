using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Migrations;

namespace WB.UI.Headquarters.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static ApplicationDbContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        public ApplicationDbContext() : base("DefaultConnection"){ }

        public static ApplicationDbContext Create() => new ApplicationDbContext();
    }
}