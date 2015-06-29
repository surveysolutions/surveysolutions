using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public interface IUserViewFactory
    {
        UserView Load(UserViewInputModel input);
    }

    public class UserViewFactory : IUserViewFactory 
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public UserViewFactory(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }
        
        public UserView Load(UserViewInputModel input)
        {
            Expression<Func<UserDocument, bool>> query = (x) => !x.IsArchived;
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
                            PhoneNumber = userDocument.PhoneNumber
                        }
                        : null;
                });
        }
    }
}