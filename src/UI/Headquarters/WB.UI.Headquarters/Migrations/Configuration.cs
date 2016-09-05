using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.UI.Headquarters.Identity;

namespace WB.UI.Headquarters.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            foreach (int userRole in Enum.GetValues(typeof(UserRoles)))
            {
                context.Roles.AddOrUpdate(new IdentityRole
                {
                    Id = userRole.ToString(),
                    Name = Enum.GetName(typeof(UserRoles), userRole)
                });
            }

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
