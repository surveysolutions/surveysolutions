using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal sealed class Configuration : DbMigrationsConfiguration<HQIdentityDbContext>
    {
        public const string SchemaName = "users";
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetHistoryContextFactory("Npgsql",
                (connection, defaultSchema) => new SchemaBasedHistoryContext(connection, defaultSchema, SchemaName));
        }

        protected override void Seed(HQIdentityDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            if (context.Roles.Any()) return;

            foreach (int userRole in Enum.GetValues(typeof(UserRoles)))
            {
                context.Roles.AddOrUpdate(new AppRole
                {
                    Id = ((byte)userRole).ToGuid(),
                    Name = Enum.GetName(typeof(UserRoles), userRole)
                });
            }

            foreach (var oldUser in ServiceLocator.Current.GetInstance<IPlainStorageAccessor<UserDocument>>().Query(users=>users.ToList()))
            {
                context.Users.AddOrUpdate(new ApplicationUser
                {
                    Id = oldUser.PublicKey,
                    CreationDate = oldUser.CreationDate,
                    DeviceId = oldUser.DeviceId,
                    Email = oldUser.Email,
                    IsArchived = oldUser.IsArchived,
                    IsLockedByHeadquaters = oldUser.IsLockedByHQ,
                    IsLockedBySupervisor = oldUser.IsLockedBySupervisor,
                    FullName = oldUser.PersonName,
                    PhoneNumber = oldUser.PhoneNumber,
                    SupervisorId = oldUser.Supervisor?.Id,
                    UserName = oldUser.UserName,
                    PasswordHash = oldUser.Password,
                    SecurityStamp = Guid.NewGuid().FormatGuid(),
                    Roles =
                    {
                        new AppUserRole
                        {
                            UserId = oldUser.PublicKey,
                            RoleId = ((byte) oldUser.Roles.First()).ToGuid()
                        }
                    }
                });
            }
        }
    }
}
