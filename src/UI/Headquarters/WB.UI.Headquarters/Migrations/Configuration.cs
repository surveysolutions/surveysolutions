using System;
using System.Data.Entity.Migrations;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Identity;

namespace WB.UI.Headquarters.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<HQIdentityDbContext>
    {
        public const string SchemaName = "system";
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            SetHistoryContextFactory("Npgsql",
                (connection, defaultSchema) => new SchemaBasedHistoryContext(connection, defaultSchema, SchemaName));
        }

        protected override void Seed(HQIdentityDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            foreach (int userRole in Enum.GetValues(typeof(UserRoles)))
            {
                context.Roles.AddOrUpdate(new AppRole
                {
                    Id = ((byte)userRole).ToGuid(),
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
