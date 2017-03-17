using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
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

        public DbSet<HqUserProfile> UserProfiles { get; set; }

        protected override void Seed(HQIdentityDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            if (context.Roles.Any()) return;

            foreach (UserRoles userRole in Enum.GetValues(typeof(UserRoles)))
            {
                context.Roles.AddOrUpdate(new HqRole
                {
                    Id = userRole.ToUserId(),
                    Name = Enum.GetName(typeof(UserRoles), userRole)
                });
            }
            var transactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();

            transactionManager.ExecuteInPlainTransaction(() =>
            {
                foreach (var oldUser in ServiceLocator.Current.GetInstance<IPlainStorageAccessor<UserDocument>>().Query(users => users.ToList()))
                {
                    context.Users.AddOrUpdate(new HqUser
                    {
                        Id = oldUser.PublicKey,
                        CreationDate = oldUser.CreationDate,
                        Email = oldUser.Email,
                        IsArchived = oldUser.IsArchived,
                        IsLockedByHeadquaters = oldUser.IsLockedByHQ,
                        IsLockedBySupervisor = oldUser.IsLockedBySupervisor,
                        FullName = oldUser.PersonName,
                        PhoneNumber = oldUser.PhoneNumber,
                        UserName = oldUser.UserName,
                        PasswordHash = oldUser.Password,
                        SecurityStamp = Guid.NewGuid().FormatGuid(),
                        Profile = oldUser.Supervisor != null || !string.IsNullOrEmpty(oldUser.DeviceId) ? new HqUserProfile
                        {
                            DeviceId = oldUser.DeviceId,
                            SupervisorId = oldUser.Supervisor?.Id,
                        } : null,
                        Roles =
                        {
                            new HqUserRole
                            {
                                UserId = oldUser.PublicKey,
                                RoleId = oldUser.Roles.First().ToUserId()
                            }
                        }
                    });
                }

            });
        }
    }
}
