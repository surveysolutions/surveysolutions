using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Migrator;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity.Migrations
{
    public class UsersToIdentityMigration : DataMigration<HQIdentityDbContext>
    {
        public override string Id => "20170406011139";

        public override void Up(HQIdentityDbContext context)
        {
            if (context.Roles.Any()) return;

            var plainTransactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
            var userPlainStorageRepository = ServiceLocator.Current.GetInstance<IPlainStorageAccessor<UserDocument>>();

            try
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                foreach (UserRoles userRole in Enum.GetValues(typeof(UserRoles)))
                {
                    context.Roles.AddOrUpdate(new HqRole
                    {
                        Id = userRole.ToUserId(),
                        Name = Enum.GetName(typeof(UserRoles), userRole)
                    });
                }

                plainTransactionManager.ExecuteInPlainTransaction(() =>
                {
                    foreach (var oldUser in userPlainStorageRepository.Query(users => users.ToList()))
                    {
                        HqUserProfile hqUserProfile = null;
                        if (oldUser.Supervisor != null || !string.IsNullOrEmpty(oldUser.DeviceId))
                            hqUserProfile = new HqUserProfile
                            {
                                DeviceId = oldUser.DeviceId,
                                SupervisorId = oldUser.Supervisor?.Id,
                            };

                        var hqUser = new HqUser
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
                            PasswordHashSha1 = oldUser.Password,
                            SecurityStamp = Guid.NewGuid().FormatGuid(),
                            Profile = hqUserProfile
                        };

                        foreach (var oldUserRole in oldUser.Roles)
                            hqUser.Roles.Add(new HqUserRole
                            {
                                UserId = oldUser.PublicKey,
                                RoleId = oldUserRole.ToUserId()
                            });

                        context.Users.Add(hqUser);
                    }
                });

                context.SaveChanges();
            }
            finally
            {
                context.Configuration.AutoDetectChangesEnabled = true;
            }
        }
    }
}