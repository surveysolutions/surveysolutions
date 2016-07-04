﻿using System;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface IUserViewFactory
    {
        UserView Load(UserViewInputModel input);
    }

    public class UserViewFactory : IUserViewFactory 
    {
        private readonly IPlainStorageAccessor<UserDocument> users;

        public UserViewFactory(IPlainStorageAccessor<UserDocument> users)
        {
            this.users = users;
        }
        
        public UserView Load(UserViewInputModel input)
        {
            Expression<Func<UserDocument, bool>> query = (x) => true;
            if (input.PublicKey != null)
            {
                query = x => x.PublicKey == input.PublicKey;
            }
            else if (!string.IsNullOrEmpty(input.UserName))
            {
                query = x => x.UserName.ToLower() == input.UserName.ToLower();
            }
            else if (!string.IsNullOrEmpty(input.UserEmail))
            {
                query = x => x.Email.ToLower() == input.UserEmail.ToLower();
            }
            else if (!string.IsNullOrEmpty(input.DeviceId))
            {
                query = x => x.DeviceId == input.DeviceId;
            }

            return
                this.users.Query(queryable =>
                {
                    UserDocument userDocument = queryable.Where(query).FirstOrDefault();

                    return userDocument != null
                        ? new UserView
                        {
                            CreationDate = userDocument.CreationDate,
                            UserName = userDocument.UserName,
                            Email = userDocument.Email,
                            IsLockedBySupervisor = userDocument.IsLockedBySupervisor,
                            IsLockedByHQ = userDocument.IsLockedByHQ,
                            PublicKey = userDocument.PublicKey,
                            Roles = userDocument.Roles,
                            Password = userDocument.Password,
                            Supervisor = userDocument.Supervisor,
                            DeviceChangingHistory = userDocument.DeviceChangingHistory,
                            PersonName = userDocument.PersonName,
                            PhoneNumber = userDocument.PhoneNumber,
                            IsArchived = userDocument.IsArchived
                        }
                        : null;
                });
        }
    }
}