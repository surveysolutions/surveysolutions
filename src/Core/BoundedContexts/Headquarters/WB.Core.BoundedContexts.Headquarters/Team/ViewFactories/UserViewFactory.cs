﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Documents;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Team.ViewFactories
{
    public class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public UserViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }
        
        public UserView Load(UserViewInputModel input)
        {
            Expression<Func<UserDocument, bool>> query = (x) => false;
            if (input.PublicKey != null)
            {
                query = x => x.PublicKey == input.PublicKey;
            }
            else if (!string.IsNullOrEmpty(input.UserName))
            {
                query = x => x.UserName == input.UserName;
            }
            else if (!string.IsNullOrEmpty(input.UserEmail))
            {
                query = x => x.Email == input.UserEmail;
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
                            IsDeleted = userDocument.IsDeleted,
                            IsLocked = userDocument.IsLocked,
                            PublicKey = userDocument.PublicKey,
                            Roles = userDocument.Roles,
                            Password = userDocument.Password,
                            Supervisor = userDocument.Supervisor,
                        }
                        : null;
                });
        }
    }
}